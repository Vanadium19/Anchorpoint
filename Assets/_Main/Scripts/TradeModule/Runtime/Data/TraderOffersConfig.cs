using System.Collections.Generic;
using UnityEngine;

namespace TradeModule
{
    /// <summary>
    /// The set of trade offers a single trader NPC exposes to the player.
    /// </summary>
    /// <remarks>
    /// Assigned to a <see cref="TraderView"/> in the scene; this is the game-design-configurable
    /// surface for defining what a given trader will buy and sell.
    /// </remarks>
    [CreateAssetMenu(fileName = "TraderOffersConfig", menuName = "Game/Configs/Trade/TraderOffers")]
    public class TraderOffersConfig : ScriptableObject
    {
        [SerializeField] private string traderNameKey = "";
        [SerializeField] private List<TradeOfferConfig> offers = new();

        /// <summary>
        /// Localization key for the trader's display name.
        /// </summary>
        public string TraderNameKey => traderNameKey;

        /// <summary>
        /// The offers this trader makes available to the player.
        /// </summary>
        public IReadOnlyList<TradeOfferConfig> Offers => offers;
    }
}
