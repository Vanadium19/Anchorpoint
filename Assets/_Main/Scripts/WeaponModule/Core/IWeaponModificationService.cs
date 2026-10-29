using System;
using System.Collections.Generic;
using InventoryModule;

namespace WeaponModule
{
    /// <summary>
    /// Reads and installs modifications of weapon item instances and calculates their effective stats.
    /// </summary>
    public interface IWeaponModificationService
    {
        event Action<ItemTable> ModificationsChanged;

        IReadOnlyList<WeaponModificationConfig> GetInstalled(ItemTable weapon);
        bool IsInstalled(ItemTable weapon, WeaponModificationConfig modification);
        bool CanInstall(ItemTable weapon, WeaponModificationConfig modification);
        bool TryInstall(ItemTable weapon, WeaponModificationConfig modification);
        WeaponStats GetStats(ItemTable weapon);
    }
}
