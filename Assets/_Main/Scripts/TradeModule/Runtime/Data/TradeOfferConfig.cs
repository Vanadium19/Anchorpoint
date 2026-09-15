using System.Collections.Generic;
using UnityEngine;

namespace TradeModule
{
    /// <summary>
    /// A single configurable trade offer: what a player must give up and what they receive in return.
    /// </summary>
    /// <remarks>
    /// A trade is atomic: <see cref="ITradeService"/> only removes <see cref="Requested"/> and grants
    /// <see cref="Offered"/> when the player holds every requested item in full.
    /// </remarks>
    [CreateAssetMenu(fileName = "TradeOfferConfig", menuName = "Game/Configs/Trade/TradeOffer")]
    public class TradeOfferConfig : ScriptableObject
    {
        [SerializeField] private string titleKey = "";
        [SerializeField] private List<TradeItemAmount> requested = new();
        [SerializeField] private List<TradeItemAmount> offered = new();

        /// <summary>
        /// Localization key for the offer's display title.
        /// </summary>
        public string TitleKey => titleKey;

        /// <summary>
        /// Items and amounts the player must give up to complete this trade.
        /// </summary>
        public IReadOnlyList<TradeItemAmount> Requested => requested;

        /// <summary>
        /// Items and amounts the player receives when this trade is completed.
        /// </summary>
        public IReadOnlyList<TradeItemAmount> Offered => offered;
    }
}
