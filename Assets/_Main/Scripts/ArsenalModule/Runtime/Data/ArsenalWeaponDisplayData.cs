using InventoryModule;
using UnityEngine;

namespace ArsenalModule
{
    /// <summary>
    /// Precomputed display data for a single owned weapon entry.
    /// </summary>
    /// <remarks>
    /// Built by <see cref="ArsenalPresenter"/> and passed to <see cref="ArsenalScreenView"/>.
    /// </remarks>
    public class ArsenalWeaponDisplayData
    {
        /// <summary>
        /// The weapon item instance this entry represents.
        /// </summary>
        public ItemTable Weapon { get; set; }

        /// <summary>
        /// Localized weapon name.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Weapon icon.
        /// </summary>
        public Sprite Icon { get; set; }

        /// <summary>
        /// Whether this weapon is the one currently being modified.
        /// </summary>
        public bool IsSelected { get; set; }
    }
}
