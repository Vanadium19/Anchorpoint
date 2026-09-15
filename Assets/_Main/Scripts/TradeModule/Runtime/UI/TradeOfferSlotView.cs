using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TradeModule
{
    /// <summary>
    /// A single trade offer row: requested items, offered items, and a button to perform the trade.
    /// </summary>
    /// <remarks>
    /// Purely presentational. It renders whatever <see cref="TradeOfferDisplayData"/> it is given and
    /// raises <see cref="Selected"/> when the player clicks the row's trade button.
    /// </remarks>
    public class TradeOfferSlotView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI titleText;

        [SerializeField] private Transform requestedContainer;
        [SerializeField] private TradeResourceSlotView requestedSlotPrefab;

        [SerializeField] private Transform offeredContainer;
        [SerializeField] private TradeResourceSlotView offeredSlotPrefab;

        [SerializeField] private Button tradeButton;
        [SerializeField] private Graphic affordabilityIndicator;

        private readonly List<TradeResourceSlotView> _requestedSlots = new();
        private readonly List<TradeResourceSlotView> _offeredSlots = new();
        private TradeOfferConfig _offer;

        /// <summary>
        /// Raised with this row's offer when the player clicks the trade button.
        /// </summary>
        public event Action<TradeOfferConfig> Selected;

        private void Awake()
        {
            if (tradeButton != null)
                tradeButton.onClick.AddListener(OnTradeClicked);
        }

        /// <summary>
        /// Renders the row from precomputed display data.
        /// </summary>
        /// <param name="data">The offer's display data, including its affordability.</param>
        public void Render(TradeOfferDisplayData data)
        {
            _offer = data.Offer;

            if (titleText != null)
                titleText.text = data.TitleText;

            RefreshSlots(_requestedSlots, requestedContainer, requestedSlotPrefab, data.RequestedItems);
            RefreshSlots(_offeredSlots, offeredContainer, offeredSlotPrefab, data.OfferedItems);

            if (tradeButton != null)
                tradeButton.interactable = data.CanAfford;

            if (affordabilityIndicator != null)
                affordabilityIndicator.color = data.CanAfford ? Color.white : Color.red;
        }

        private void OnTradeClicked() => Selected?.Invoke(_offer);

        private static void RefreshSlots(
            List<TradeResourceSlotView> slots,
            Transform container,
            TradeResourceSlotView prefab,
            IReadOnlyList<TradeItemAmount> items)
        {
            if (container == null || prefab == null || items == null)
                return;

            while (slots.Count < items.Count)
                slots.Add(Instantiate(prefab, container));

            for (var i = 0; i < slots.Count; i++)
            {
                var isVisible = i < items.Count;
                slots[i].gameObject.SetActive(isVisible);

                if (isVisible)
                    slots[i].Render(items[i]);
            }
        }
    }
}
