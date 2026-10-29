using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WeaponModule;

namespace ArsenalModule
{
    /// <summary>
    /// A single modification row: title, description, cost and a button to install it.
    /// </summary>
    /// <remarks>
    /// Purely presentational. It renders whatever <see cref="ArsenalModificationDisplayData"/> it is given
    /// and raises <see cref="InstallRequested"/> when the player clicks the install button.
    /// </remarks>
    public class ArsenalModificationSlotView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        [SerializeField] private Transform costContainer;
        [SerializeField] private ArsenalCostSlotView costSlotPrefab;

        [SerializeField] private Button installButton;
        [SerializeField] private GameObject installedLabel;
        [SerializeField] private GameObject slotOccupiedLabel;

        private readonly List<ArsenalCostSlotView> _costSlots = new();
        private WeaponModificationConfig _modification;

        /// <summary>
        /// Raised with this row's modification when the player clicks the install button.
        /// </summary>
        public event Action<WeaponModificationConfig> InstallRequested;

        private void Awake()
        {
            if (installButton != null)
                installButton.onClick.AddListener(OnButtonClicked);
        }

        private void OnDestroy()
        {
            if (installButton != null)
                installButton.onClick.RemoveListener(OnButtonClicked);
        }

        /// <summary>
        /// Renders the row from precomputed display data.
        /// </summary>
        /// <param name="data">The modification's display data.</param>
        public void Render(ArsenalModificationDisplayData data)
        {
            _modification = data.Modification;

            if (iconImage != null)
                iconImage.sprite = data.Icon;

            if (titleText != null)
                titleText.text = data.Title;

            if (descriptionText != null)
                descriptionText.text = data.Description;

            RenderCost(data);

            if (installButton != null)
            {
                installButton.gameObject.SetActive(!data.IsInstalled && !data.IsSlotOccupied);
                installButton.interactable = data.CanInstall;
            }

            if (installedLabel != null)
                installedLabel.SetActive(data.IsInstalled);

            if (slotOccupiedLabel != null)
                slotOccupiedLabel.SetActive(data.IsSlotOccupied);
        }

        private void RenderCost(ArsenalModificationDisplayData data)
        {
            if (costContainer == null || costSlotPrefab == null || data.Cost == null)
                return;

            costContainer.gameObject.SetActive(!data.IsInstalled);

            while (_costSlots.Count < data.Cost.Count)
                _costSlots.Add(Instantiate(costSlotPrefab, costContainer));

            for (var i = 0; i < _costSlots.Count; i++)
            {
                var isVisible = i < data.Cost.Count;
                _costSlots[i].gameObject.SetActive(isVisible);

                if (isVisible)
                    _costSlots[i].Render(data.Cost[i]);
            }
        }

        private void OnButtonClicked() => InstallRequested?.Invoke(_modification);
    }
}
