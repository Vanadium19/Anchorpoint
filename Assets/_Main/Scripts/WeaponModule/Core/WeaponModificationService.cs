using System;
using System.Collections.Generic;
using InventoryModule;

namespace WeaponModule
{
    public class WeaponModificationService : IWeaponModificationService
    {
        public event Action<ItemTable> ModificationsChanged;

        public IReadOnlyList<WeaponModificationConfig> GetInstalled(ItemTable weapon)
        {
            var installed = new List<WeaponModificationConfig>();

            if (weapon?.ItemDataSo is not WeaponItemSo weaponItem || weapon.WeaponModificationMetadata == null)
                return installed;

            foreach (var modificationID in weapon.WeaponModificationMetadata.InstalledModificationIDs)
            {
                if (weaponItem.TryGetModification(modificationID, out var modification))
                    installed.Add(modification);
            }

            return installed;
        }

        public bool IsInstalled(ItemTable weapon, WeaponModificationConfig modification)
        {
            if (weapon?.WeaponModificationMetadata == null || modification == null)
                return false;

            return weapon.WeaponModificationMetadata.Contains(modification.ID);
        }

        public bool CanInstall(ItemTable weapon, WeaponModificationConfig modification)
        {
            if (weapon?.ItemDataSo is not WeaponItemSo weaponItem || modification == null)
                return false;

            if (!weaponItem.TryGetModification(modification.ID, out _))
                return false;

            foreach (var installed in GetInstalled(weapon))
            {
                if (installed.Slot == modification.Slot)
                    return false;
            }

            return true;
        }

        public bool TryInstall(ItemTable weapon, WeaponModificationConfig modification)
        {
            if (!CanInstall(weapon, modification))
                return false;

            if (!weapon.GetOrCreateWeaponModifications().TryAdd(modification.ID))
                return false;

            ModificationsChanged?.Invoke(weapon);
            return true;
        }

        public WeaponStats GetStats(ItemTable weapon)
        {
            if (weapon?.ItemDataSo is not WeaponItemSo weaponItem || weaponItem.WeaponConfig == null)
                throw new ArgumentException("Item is not a configured weapon.", nameof(weapon));

            return new WeaponStats(weaponItem.WeaponConfig, GetInstalled(weapon));
        }
    }
}
