using System.Collections.Generic;
using UnityEngine;
using WeaponModule;

namespace ArsenalModule
{
    /// <summary>
    /// Precomputed display data for a single modification row.
    /// </summary>
    public class ArsenalModificationDisplayData
    {
        /// <summary>
        /// The modification this row represents.
        /// </summary>
        public WeaponModificationConfig Modification { get; set; }

        /// <summary>
        /// Localized modification name.
        /// </summary>
        public string NameText { get; set; }

        /// <summary>
        /// Localized modification description.
        /// </summary>
        public string DescriptionText { get; set; }

        /// <summary>
        /// Icon of the modification.
        /// </summary>
        public Sprite Icon { get; set; }

        /// <summary>
        /// Localized name of the slot the modification occupies.
        /// </summary>
        public string SlotText { get; set; }

        /// <summary>
        /// Install status of the modification on the selected weapon.
        /// </summary>
        public ArsenalInstallStatus Status { get; set; }

        /// <summary>
        /// Localized text describing <see cref="Status"/>.
        /// </summary>
        public string StatusText { get; set; }

        /// <summary>
        /// Whether the modification is already installed on the selected weapon.
        /// </summary>
        public bool IsInstalled { get; set; }

        /// <summary>
        /// Whether this modification is the one currently selected.
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Whether the modification can be installed right now.
        /// </summary>
        public bool CanInstall { get; set; }

        /// <summary>
        /// Resources the installation costs, with owned and required counts.
        /// </summary>
        public IReadOnlyList<ArsenalCostDisplayData> Costs { get; set; }
    }
}
