using System.Collections.Generic;

namespace WeaponModule
{
    public class WeaponStatsProvider : IWeaponStatsProvider
    {
        public WeaponConfig GetConfig(WeaponItemSo weaponItem)
        {
            return weaponItem != null ? weaponItem.WeaponConfig : null;
        }

        public WeaponBuild GetBuild(WeaponItemSo weaponItem, IReadOnlyList<string> modificationIds)
        {
            var config = GetConfig(weaponItem);

            if (config == null)
                return null;

            var installed = new List<WeaponModificationConfig>();

            if (modificationIds != null)
            {
                foreach (var id in modificationIds)
                {
                    var modification = FindModification(weaponItem, id);

                    if (modification != null && !installed.Contains(modification))
                        installed.Add(modification);
                }
            }

            return WeaponBuild.Create(config, installed);
        }

        private static WeaponModificationConfig FindModification(WeaponItemSo weaponItem, string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            foreach (var modification in weaponItem.AvailableModifications)
            {
                if (modification != null && modification.Id == id)
                    return modification;
            }

            return null;
        }
    }
}
