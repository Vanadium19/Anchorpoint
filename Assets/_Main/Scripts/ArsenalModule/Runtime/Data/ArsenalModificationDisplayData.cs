using System.Collections.Generic;
using UnityEngine;
using WeaponModule;

namespace ArsenalModule
{
    /// <summary>
    /// Precomputed display data for a single modification row.
    /// </summary>
    /// <remarks>
    /// Built by <see cref="ArsenalPresenter"/> and passed to <see cref="ArsenalScreenView"/> so the view
    /// never needs to resolve text, prices or availability itself.
    /// </remarks>
    public class ArsenalModificationDisplayData
    {
        /// <summary>
        /// The modification this row represents.
        /// </summary>
        public WeaponModificationConfig Modification { get; set; }

        /// <summary>
        /// Localized modification title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Localized modification description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Modification icon.
        /// </summary>
        public Sprite Icon { get; set; }

        /// <summary>
        /// Items and amounts the player pays, for display.
        /// </summary>
        public IReadOnlyList<ArsenalCostDisplayData> Cost { get; set; }

        /// <summary>
        /// Whether the selected weapon already has this modification.
        /// </summary>
        public bool IsInstalled { get; set; }

        /// <summary>
        /// Whether another modification already occupies this modification's slot on the selected weapon.
        /// </summary>
        public bool IsSlotOccupied { get; set; }

        /// <summary>
        /// Whether the modification can be installed right now, including affordability.
        /// </summary>
        public bool CanInstall { get; set; }
    }
}
