using UnityEngine;

namespace ArsenalModule
{
    /// <summary>
    /// Precomputed display data for one resource a modification costs.
    /// </summary>
    public class ArsenalCostDisplayData
    {
        /// <summary>
        /// Localized name of the resource item.
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// Icon of the resource item.
        /// </summary>
        public Sprite Icon { get; set; }

        /// <summary>
        /// How many of the resource the player holds.
        /// </summary>
        public int Owned { get; set; }

        /// <summary>
        /// How many of the resource the installation requires.
        /// </summary>
        public int Required { get; set; }

        /// <summary>
        /// Whether the player holds at least the required amount.
        /// </summary>
        public bool IsEnough { get; set; }
    }
}
