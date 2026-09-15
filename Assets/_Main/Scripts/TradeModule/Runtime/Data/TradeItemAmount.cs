using System;
using InventoryModule;
using UnityEngine;

namespace TradeModule
{
    /// <summary>
    /// A quantity of a single inventory item used inside a trade offer.
    /// </summary>
    /// <remarks>
    /// Instances are authored on <see cref="TradeOfferConfig"/> to describe either the items a player
    /// must give up or the items a player receives when a trade is completed.
    /// </remarks>
    [Serializable]
    public class TradeItemAmount
    {
        [SerializeField] private ItemDataSo item;
        [SerializeField] private int count = 1;

        /// <summary>
        /// The item this amount refers to.
        /// </summary>
        public ItemDataSo Item => item;

        /// <summary>
        /// How many units of <see cref="Item"/> this amount represents.
        /// </summary>
        public int Count => count;
    }
}
