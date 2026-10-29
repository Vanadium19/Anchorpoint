using System.Collections.Generic;
using UnityEngine;
using WeaponModule;

namespace ArsenalModule
{
    /// <summary>
    /// The modifications an Arsenal sells and their prices.
    /// </summary>
    /// <remarks>
    /// Assigned to an <see cref="ArsenalStationView"/> in the scene; this is the game-design-configurable
    /// surface for pricing. A modification without a price entry is not sold at the Arsenal.
    /// </remarks>
    [CreateAssetMenu(fileName = "ArsenalConfig", menuName = "Game/Configs/Arsenal/ArsenalConfig")]
    public class ArsenalConfig : ScriptableObject
    {
        [SerializeField] private string titleKey = "";
        [SerializeField] private List<ArsenalModificationPrice> prices = new();

        /// <summary>
        /// Localization key for the Arsenal screen title.
        /// </summary>
        public string TitleKey => titleKey;

        /// <summary>
        /// Prices of every modification sold at this Arsenal.
        /// </summary>
        public IReadOnlyList<ArsenalModificationPrice> Prices => prices;

        /// <summary>
        /// Finds the price of a modification.
        /// </summary>
        /// <param name="modification">The modification to look up.</param>
        /// <param name="price">The price entry, or null if the modification is not sold here.</param>
        /// <returns>True if the Arsenal sells <paramref name="modification"/>.</returns>
        public bool TryGetPrice(WeaponModificationConfig modification, out ArsenalModificationPrice price)
        {
            price = null;

            if (modification == null)
                return false;

            foreach (var entry in prices)
            {
                if (entry == null || entry.Modification != modification)
                    continue;

                price = entry;
                return true;
            }

            return false;
        }
    }
}
