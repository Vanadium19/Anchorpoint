using System.Collections.Generic;

namespace WeaponModule
{
    public interface IWeaponStatsProvider
    {
        WeaponConfig GetConfig(WeaponItemSo weaponItem);

        /// <summary>
        /// Builds the weapon with the installed modifications. Ids that are not among the item's available
        /// modifications are skipped. Returns null only when the item or its config is missing.
        /// </summary>
        WeaponBuild GetBuild(WeaponItemSo weaponItem, IReadOnlyList<string> modificationIds);
    }
}
