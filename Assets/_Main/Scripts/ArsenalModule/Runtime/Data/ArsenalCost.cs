using System;
using InventoryModule;
using UnityEngine;

namespace ArsenalModule
{
    /// <summary>
    /// A quantity of a single inventory item the player pays to install a weapon modification.
    /// </summary>
    /// <remarks>
    /// Authored on <see cref="ArsenalModificationPrice"/>; the amount is set by game design.
    /// </remarks>
    [Serializable]
    public class ArsenalCost
    {
        [SerializeField] private ItemDataSo item;
        [SerializeField] private int count = 1;

        /// <summary>
        /// The item the player pays with.
        /// </summary>
        public ItemDataSo Item => item;

        /// <summary>
        /// How many units of <see cref="Item"/> are paid.
        /// </summary>
        public int Count => count;
    }
}
