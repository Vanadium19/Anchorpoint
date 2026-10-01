using System.Collections.Generic;

namespace TradeModule
{
    /// <summary>
    /// Precomputed display data for a single trade offer row.
    /// </summary>
    /// <remarks>
    /// Built by <see cref="TradePresenter"/> and passed to <see cref="TradeView"/> so the view never
    /// needs to resolve items, prices or affordability itself.
    /// </remarks>
    public class TradeOfferDisplayData
    {
        /// <summary>
        /// The offer this display data was built from.
        /// </summary>
        public TradeOfferConfig Offer { get; set; }

        /// <summary>
        /// Localized title text for the offer.
        /// </summary>
        public string TitleText { get; set; }

        /// <summary>
        /// Items and amounts the player must give up, for display.
        /// </summary>
        public IReadOnlyList<TradeItemAmount> RequestedItems { get; set; }

        /// <summary>
        /// Items and amounts the player receives, for display.
        /// </summary>
        public IReadOnlyList<TradeItemAmount> OfferedItems { get; set; }

        /// <summary>
        /// Whether the player currently holds everything required to complete this trade.
        /// </summary>
        public bool CanAfford { get; set; }
    }
}
