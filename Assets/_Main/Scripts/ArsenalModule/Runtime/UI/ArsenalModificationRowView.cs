using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WeaponModule;

namespace ArsenalModule
{
    /// <summary>
    /// A single modification row: icon, name, description, slot, install status, costs and a select button.
    /// </summary>
    /// <remarks>
    /// Purely presentational. It renders whatever <see cref="ArsenalModificationDisplayData"/> it is given
    /// and raises <see cref="Selected"/> when the player clicks the row.
    /// </remarks>
    public class ArsenalModificationRowView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI slotText;
        [SerializeField] private TextMeshProUGUI statusText;

        [SerializeField] private Transform costsContainer;
        [SerializeField] private ArsenalCostRowView costRowPrefab;

        [SerializeField] private Button selectButton;
        [SerializeField] private GameObject selectedIndicator;
        [SerializeField] private GameObject installedIndicator;

        private readonly List<ArsenalCostRowView> _costRows = new();
        private WeaponModificationConfig _modification;

        /// <summary>
        /// Raised with this row's modification when the player clicks the row.
        /// </summary>
        public event Action<WeaponModificationConfig> Selected;

        private void Awake()
        {
            if (selectButton != null)
                selectButton.onClick.AddListener(OnSelectClicked);
        }

        private void OnDestroy()
        {
            if (selectButton != null)
                selectButton.onClick.RemoveListener(OnSelectClicked);
        }

        /// <summary>
        /// Renders the row from precomputed display data.
        /// </summary>
        /// <param name="data">The modification's display data, including status and costs.</param>
        public void Render(ArsenalModificationDisplayData data)
        {
            _modification = data.Modification;

            if (iconImage != null)
            {
                iconImage.sprite = data.Icon;
                iconImage.enabled = data.Icon != null;
            }

            if (nameText != null)
                nameText.text = data.NameText;

            if (descriptionText != null)
                descriptionText.text = data.DescriptionText;

            if (slotText != null)
                slotText.text = data.SlotText;

            if (statusText != null)
                statusText.text = data.StatusText;

            if (selectedIndicator != null)
                selectedIndicator.SetActive(data.IsSelected);

            if (installedIndicator != null)
                installedIndicator.SetActive(data.IsInstalled);

            RenderCosts(data.Costs);
        }

        private void RenderCosts(IReadOnlyList<ArsenalCostDisplayData> costs)
        {
            if (costsContainer == null || costRowPrefab == null || costs == null)
                return;

            while (_costRows.Count < costs.Count)
                _costRows.Add(Instantiate(costRowPrefab, costsContainer));

            for (var i = 0; i < _costRows.Count; i++)
            {
                var isVisible = i < costs.Count;
                _costRows[i].gameObject.SetActive(isVisible);

                if (isVisible)
                    _costRows[i].Render(costs[i]);
            }
        }

        private void OnSelectClicked() => Selected?.Invoke(_modification);
    }
}
