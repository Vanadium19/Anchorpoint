using InventoryModule;

namespace ArsenalModule
{
    /// <summary>
    /// Precomputed display data for a single weapon row.
    /// </summary>
    public class ArsenalWeaponDisplayData
    {
        /// <summary>
        /// The weapon item this row represents.
        /// </summary>
        public ItemTable Weapon { get; set; }

        /// <summary>
        /// Localized weapon name.
        /// </summary>
        public string NameText { get; set; }

        /// <summary>
        /// Whether this weapon is the one currently selected.
        /// </summary>
        public bool IsSelected { get; set; }
    }
}
