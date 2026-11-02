using System;
using System.Collections.Generic;
using InventoryModule;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WeaponModule;

namespace ArsenalModule
{
    /// <summary>
    /// The Arsenal screen: a weapon list, the modification list of the selected weapon, a stat comparison
    /// and an install button.
    /// </summary>
    /// <remarks>
    /// Follows MVP strictly: it holds no reference to <c>InventoryModule.IInventoryManager</c> or
    /// <see cref="IArsenalService"/>. All data reaches it already prepared through <see cref="SetTexts"/>
    /// and <see cref="Render"/>, and all it does in return is raise events for <see cref="ArsenalPresenter"/>.
    /// </remarks>
    public class ArsenalScreenView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI weaponsHeaderText;
        [SerializeField] private TextMeshProUGUI modificationsHeaderText;
        [SerializeField] private TextMeshProUGUI statsHeaderText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private GameObject messageRoot;
        [SerializeField] private GameObject detailsRoot;

        [SerializeField] private Transform weaponsContainer;
        [SerializeField] private ArsenalWeaponRowView weaponRowPrefab;

        [SerializeField] private Transform modificationsContainer;
        [SerializeField] private ArsenalModificationRowView modificationRowPrefab;

        [SerializeField] private Transform statsContainer;
        [SerializeField] private ArsenalStatRowView statRowPrefab;

        [SerializeField] private Button installButton;
        [SerializeField] private TextMeshProUGUI installButtonText;

        private readonly List<ArsenalWeaponRowView> _weaponRows = new();
        private readonly List<ArsenalModificationRowView> _modificationRows = new();
        private readonly List<ArsenalStatRowView> _statRows = new();

        /// <summary>
        /// Raised with the corresponding weapon when the player selects a weapon row.
        /// </summary>
        public event Action<ItemTable> WeaponSelected;

        /// <summary>
        /// Raised with the corresponding modification when the player selects a modification row.
        /// </summary>
        public event Action<WeaponModificationConfig> ModificationSelected;

        /// <summary>
        /// Raised when the player clicks the install button.
        /// </summary>
        public event Action InstallRequested;

        /// <summary>
        /// Raised when this view is destroyed, so its presenter can unsubscribe from external events.
        /// </summary>
        public event Action Closed;

        private void Awake()
        {
            if (installButton != null)
                installButton.onClick.AddListener(OnInstallClicked);
        }

        private void OnDestroy()
        {
            if (installButton != null)
                installButton.onClick.RemoveListener(OnInstallClicked);

            foreach (var row in _weaponRows)
                row.Selected -= OnWeaponRowSelected;

            foreach (var row in _modificationRows)
                row.Selected -= OnModificationRowSelected;

            Closed?.Invoke();
        }

        /// <summary>
        /// Sets the static localized texts of the screen.
        /// </summary>
        /// <param name="texts">The title, headers and button caption.</param>
        public void SetTexts(ArsenalScreenTexts texts)
        {
            if (titleText != null)
                titleText.text = texts.TitleText;

            if (weaponsHeaderText != null)
                weaponsHeaderText.text = texts.WeaponsHeaderText;

            if (modificationsHeaderText != null)
                modificationsHeaderText.text = texts.ModificationsHeaderText;

            if (statsHeaderText != null)
                statsHeaderText.text = texts.StatsHeaderText;

            if (installButtonText != null)
                installButtonText.text = texts.InstallButtonText;
        }

        /// <summary>
        /// Renders the whole screen from precomputed display data.
        /// </summary>
        /// <param name="data">Display data for weapons, modifications, stats and empty states.</param>
        public void Render(ArsenalScreenDisplayData data)
        {
            RenderWeapons(data.Weapons);
            RenderModifications(data.Modifications);
            RenderStats(data.Stats);

            var hasMessage = !string.IsNullOrEmpty(data.MessageText);

            if (messageText != null)
                messageText.text = data.MessageText;

            if (messageRoot != null)
                messageRoot.SetActive(hasMessage);

            if (detailsRoot != null)
                detailsRoot.SetActive(data.HasSelectedWeapon);

            if (installButton != null)
                installButton.interactable = data.CanInstall;
        }

        private static void EnsureRows<TRow>(
            List<TRow> rows,
            TRow prefab,
            Transform container,
            int count,
            Action<TRow> onCreated)
            where TRow : MonoBehaviour
        {
            if (container == null || prefab == null)
                return;

            while (rows.Count < count)
            {
                var row = Instantiate(prefab, container);
                onCreated?.Invoke(row);
                rows.Add(row);
            }
        }

        private void RenderWeapons(IReadOnlyList<ArsenalWeaponDisplayData> weapons)
        {
            EnsureRows(_weaponRows, weaponRowPrefab, weaponsContainer, weapons.Count, SubscribeWeaponRow);

            for (var i = 0; i < _weaponRows.Count; i++)
            {
                var isVisible = i < weapons.Count;
                _weaponRows[i].gameObject.SetActive(isVisible);

                if (isVisible)
                    _weaponRows[i].Render(weapons[i]);
            }
        }

        private void RenderModifications(IReadOnlyList<ArsenalModificationDisplayData> modifications)
        {
            EnsureRows(
                _modificationRows,
                modificationRowPrefab,
                modificationsContainer,
                modifications.Count,
                SubscribeModificationRow);

            for (var i = 0; i < _modificationRows.Count; i++)
            {
                var isVisible = i < modifications.Count;
                _modificationRows[i].gameObject.SetActive(isVisible);

                if (isVisible)
                    _modificationRows[i].Render(modifications[i]);
            }
        }

        private void RenderStats(IReadOnlyList<ArsenalStatDisplayData> stats)
        {
            EnsureRows(_statRows, statRowPrefab, statsContainer, stats.Count, null);

            for (var i = 0; i < _statRows.Count; i++)
            {
                var isVisible = i < stats.Count;
                _statRows[i].gameObject.SetActive(isVisible);

                if (isVisible)
                    _statRows[i].Render(stats[i]);
            }
        }

        private void SubscribeWeaponRow(ArsenalWeaponRowView row) => row.Selected += OnWeaponRowSelected;

        private void SubscribeModificationRow(ArsenalModificationRowView row) =>
            row.Selected += OnModificationRowSelected;

        private void OnWeaponRowSelected(ItemTable weapon) => WeaponSelected?.Invoke(weapon);

        private void OnModificationRowSelected(WeaponModificationConfig modification) =>
            ModificationSelected?.Invoke(modification);

        private void OnInstallClicked() => InstallRequested?.Invoke();
    }
}
