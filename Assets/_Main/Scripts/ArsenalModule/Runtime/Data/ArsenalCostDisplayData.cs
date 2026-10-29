using UnityEngine;

namespace ArsenalModule
{
    /// <summary>
    /// Precomputed display data for a single cost entry of a modification.
    /// </summary>
    /// <remarks>
    /// Built by <see cref="ArsenalPresenter"/> and passed to <see cref="ArsenalCostSlotView"/>.
    /// </remarks>
    public class ArsenalCostDisplayData
    {
        /// <summary>
        /// Icon of the item the player pays with.
        /// </summary>
        public Sprite Icon { get; set; }

        /// <summary>
        /// Required amount of the item.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Whether the player currently holds the required amount.
        /// </summary>
        public bool HasEnough { get; set; }
    }
}
