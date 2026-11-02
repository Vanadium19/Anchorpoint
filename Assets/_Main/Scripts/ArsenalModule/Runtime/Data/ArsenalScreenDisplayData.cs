using System.Collections.Generic;

namespace ArsenalModule
{
    /// <summary>
    /// Precomputed display data for the whole Arsenal screen.
    /// </summary>
    /// <remarks>
    /// Built by <see cref="ArsenalPresenter"/> and passed to <see cref="ArsenalScreenView"/> so the view
    /// never needs to resolve weapons, builds, costs or localization itself.
    /// </remarks>
    public class ArsenalScreenDisplayData
    {
        /// <summary>
        /// A row for every weapon the player carries.
        /// </summary>
        public IReadOnlyList<ArsenalWeaponDisplayData> Weapons { get; set; }

        /// <summary>
        /// A row for every modification available for the selected weapon.
        /// </summary>
        public IReadOnlyList<ArsenalModificationDisplayData> Modifications { get; set; }

        /// <summary>
        /// A line for every stat of the selected weapon.
        /// </summary>
        public IReadOnlyList<ArsenalStatDisplayData> Stats { get; set; }

        /// <summary>
        /// Localized message explaining an empty state, or an empty string when there is nothing to explain.
        /// </summary>
        public string MessageText { get; set; }

        /// <summary>
        /// Whether a weapon is selected, so the modification list and stat comparison are shown.
        /// </summary>
        public bool HasSelectedWeapon { get; set; }

        /// <summary>
        /// Whether the selected modification can be installed on the selected weapon right now.
        /// </summary>
        public bool CanInstall { get; set; }
    }
}
