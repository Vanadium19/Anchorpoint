using System;
using UnityEngine;

namespace BaseModule
{
    /// <summary>
    /// Gameplay effect granted once when a building reaches the upgrade level that owns it.
    /// Derive from this class in any module to add a new kind of upgrade.
    /// </summary>
    [Serializable]
    public abstract class BuildingUpgradeEffect
    {
        /// <summary>
        /// Localized text shown in the upgrade HUD before the level is bought; empty when the effect does not apply.
        /// </summary>
        public abstract string GetDescription(GameObject building);

        /// <summary>
        /// Applies the effect to the building instance.
        /// </summary>
        public abstract void Apply(GameObject building);
    }
}
