using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TradeModule
{
    /// <summary>
    /// The trader screen: a list of trade offer rows built from a slot prefab.
    /// </summary>
    /// <remarks>
    /// Follows MVP strictly: it holds no reference to <c>InventoryModule.IInventoryManager</c> or
    /// <see cref="ITradeService"/>. All data reaches it already prepared through <see cref="Render"/>,
    /// and all it does in return is raise events for <see cref="TradePresenter"/> to act on.
    /// </remarks>
    public class TradeView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI traderNameText;
        [SerializeField] private Transform offersContainer;
        [SerializeField] private TradeOfferSlotView offerSlotPrefab;

        private readonly List<TradeOfferSlotView> _slots = new();

        /// <summary>
        /// Raised with the corresponding offer when the player selects a trade offer row.
        /// </summary>
        public event Action<TradeOfferConfig> OfferSelected;

        /// <summary>
        /// Raised when this view is destroyed, so its presenter can unsubscribe from external events.
        /// </summary>
        public event Action Closed;

        private void OnDestroy()
        {
            foreach (var slot in _slots)
                slot.Selected -= OnSlotSelected;

            Closed?.Invoke();
        }

        /// <summary>
        /// Sets the trader's display name shown at the top of the screen.
        /// </summary>
        /// <param name="traderName">The localized trader name.</param>
        public void SetTraderName(string traderName)
        {
            if (traderNameText != null)
                traderNameText.text = traderName;
        }

        /// <summary>
        /// Renders every offer row from precomputed display data.
        /// </summary>
        /// <param name="offers">Display data for every offer this trader exposes.</param>
        public void Render(IReadOnlyList<TradeOfferDisplayData> offers)
        {
            EnsureSlots(offers.Count);

            for (var i = 0; i < _slots.Count; i++)
            {
                var isVisible = i < offers.Count;
                _slots[i].gameObject.SetActive(isVisible);

                if (isVisible)
                    _slots[i].Render(offers[i]);
            }
        }

        private void EnsureSlots(int count)
        {
            if (offersContainer == null || offerSlotPrefab == null)
                return;

            while (_slots.Count < count)
            {
                var slot = Instantiate(offerSlotPrefab, offersContainer);
                slot.Selected += OnSlotSelected;
                _slots.Add(slot);
            }
        }

        private void OnSlotSelected(TradeOfferConfig offer) => OfferSelected?.Invoke(offer);
    }
}
