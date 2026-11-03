using System.Collections.Generic;

namespace WeaponModule
{
    public interface IWeaponStatsProvider
    {
        WeaponConfig GetConfig(WeaponItemSo weaponItem);

        /// <summary>
        /// Returns the catalog modifications that fit the weapon: the ones whose socket type the weapon view has.
        /// The list is cached per weapon item, so repeated calls do not allocate.
        /// </summary>
        IReadOnlyList<WeaponModificationConfig> GetAvailableModifications(WeaponItemSo weaponItem);

        /// <summary>
        /// Builds the weapon with the installed modifications. Ids that are not in the catalog or whose socket type
        /// the weapon lacks are skipped. Returns null only when the item or its config is missing.
        /// </summary>
        WeaponBuild GetBuild(WeaponItemSo weaponItem, IReadOnlyList<string> modificationIds);
    }
}
