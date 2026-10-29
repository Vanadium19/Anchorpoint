using System;
using System.Collections.Generic;
using InventoryModule;
using TMPro;
using UnityEngine;
using WeaponModule;

namespace ArsenalModule
{
    /// <summary>
    /// The Arsenal screen: a list of owned weapons and the modification rows of the selected weapon.
    /// </summary>
    /// <remarks>
    /// Follows MVP strictly: it holds no reference to <see cref="IArsenalService"/> or
    /// <c>InventoryModule.IInventoryManager</c>. All data reaches it already prepared through the
    /// <c>Render</c> methods, and all it does in return is raise events for
    /// <see cref="ArsenalPresenter"/> to act on.
    /// </remarks>
    public class ArsenalScreenView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI selectedWeaponText;
        [SerializeField] private GameObject emptyState;

        [SerializeField] private Transform weaponsContainer;
        [SerializeField] private ArsenalWeaponSlotView weaponSlotPrefab;

        [SerializeField] private Transform modificationsContainer;
        [SerializeField] private ArsenalModificationSlotView modificationSlotPrefab;

        private readonly List<ArsenalWeaponSlotView> _weaponSlots = new();
        private readonly List<ArsenalModificationSlotView> _modificationSlots = new();

        /// <summary>
        /// Raised with the corresponding weapon when the player selects a weapon entry.
        /// </summary>
        public event Action<ItemTable> WeaponSelected;

        /// <summary>
        /// Raised with the corresponding modification when the player clicks its install button.
        /// </summary>
        public event Action<WeaponModificationConfig> InstallRequested;

        /// <summary>
        /// Raised when this view is destroyed, so its presenter can unsubscribe from external events.
        /// </summary>
        public event Action Closed;

        private void OnDestroy()
        {
            foreach (var slot in _weaponSlots)
                slot.Selected -= OnWeaponSlotSelected;

            foreach (var slot in _modificationSlots)
                slot.InstallRequested -= OnModificationInstallRequested;

            Closed?.Invoke();
        }

        /// <summary>
        /// Sets the screen title.
        /// </summary>
        /// <param name="title">The localized title.</param>
        public void SetTitle(string title)
        {
            if (titleText != null)
                titleText.text = title;
        }

        /// <summary>
        /// Renders an entry for every owned weapon.
        /// </summary>
        /// <param name="weapons">Display data for every owned weapon.</param>
        public void RenderWeapons(IReadOnlyList<ArsenalWeaponDisplayData> weapons)
        {
            EnsureWeaponSlots(weapons.Count);

            for (var i = 0; i < _weaponSlots.Count; i++)
            {
                var isVisible = i < weapons.Count;
                _weaponSlots[i].gameObject.SetActive(isVisible);

                if (isVisible)
                    _weaponSlots[i].Render(weapons[i]);
            }
        }

        /// <summary>
        /// Shows the name of the weapon being modified.
        /// </summary>
        /// <param name="weaponTitle">The localized weapon name.</param>
        public void RenderSelectedWeapon(string weaponTitle)
        {
            if (selectedWeaponText != null)
                selectedWeaponText.text = weaponTitle;
        }

        /// <summary>
        /// Renders a row for every modification offered for the selected weapon.
        /// </summary>
        /// <param name="modifications">Display data for every offered modification.</param>
        public void RenderModifications(IReadOnlyList<ArsenalModificationDisplayData> modifications)
        {
            EnsureModificationSlots(modifications.Count);

            for (var i = 0; i < _modificationSlots.Count; i++)
            {
                var isVisible = i < modifications.Count;
                _modificationSlots[i].gameObject.SetActive(isVisible);

                if (isVisible)
                    _modificationSlots[i].Render(modifications[i]);
            }
        }

        /// <summary>
        /// Shows or hides the message displayed when the player owns no weapons.
        /// </summary>
        /// <param name="isEmpty">True if the player owns no weapons.</param>
        public void RenderEmptyState(bool isEmpty)
        {
            if (emptyState != null)
                emptyState.SetActive(isEmpty);

            if (selectedWeaponText != null)
                selectedWeaponText.gameObject.SetActive(!isEmpty);
        }

        private void EnsureWeaponSlots(int count)
        {
            if (weaponsContainer == null || weaponSlotPrefab == null)
                return;

            while (_weaponSlots.Count < count)
            {
                var slot = Instantiate(weaponSlotPrefab, weaponsContainer);
                slot.Selected += OnWeaponSlotSelected;
                _weaponSlots.Add(slot);
            }
        }

        private void EnsureModificationSlots(int count)
        {
            if (modificationsContainer == null || modificationSlotPrefab == null)
                return;

            while (_modificationSlots.Count < count)
            {
                var slot = Instantiate(modificationSlotPrefab, modificationsContainer);
                slot.InstallRequested += OnModificationInstallRequested;
                _modificationSlots.Add(slot);
            }
        }

        private void OnWeaponSlotSelected(ItemTable weapon) => WeaponSelected?.Invoke(weapon);

        private void OnModificationInstallRequested(WeaponModificationConfig modification) =>
            InstallRequested?.Invoke(modification);
    }
}
