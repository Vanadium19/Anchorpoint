using System;
using System.Collections.Generic;

namespace WeaponModule
{
    public class WeaponStatsProvider : IWeaponStatsProvider
    {
        private readonly WeaponModificationCatalog _catalog;
        private readonly Dictionary<WeaponItemSo, IReadOnlyList<WeaponModificationConfig>> _available = new();

        public WeaponStatsProvider(WeaponModificationCatalog catalog)
        {
            _catalog = catalog != null
                ? catalog
                : throw new ArgumentNullException(
                    nameof(catalog),
                    $"{nameof(WeaponModificationCatalog)} is missing. Assign it on the WeaponInstaller.");
        }

        public WeaponConfig GetConfig(WeaponItemSo weaponItem)
        {
            return weaponItem != null ? weaponItem.WeaponConfig : null;
        }

        public IReadOnlyList<WeaponModificationConfig> GetAvailableModifications(WeaponItemSo weaponItem)
        {
            if (weaponItem == null)
                return Array.Empty<WeaponModificationConfig>();

            if (_available.TryGetValue(weaponItem, out var available))
                return available;

            available = FindAvailable(weaponItem);
            _available[weaponItem] = available;

            return available;
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
                    var modification = _catalog.FindById(id);

                    if (modification != null && weaponItem.HasSocket(modification.SocketType)
                        && !installed.Contains(modification))
                    {
                        installed.Add(modification);
                    }
                }
            }

            return WeaponBuild.Create(config, installed);
        }

        private IReadOnlyList<WeaponModificationConfig> FindAvailable(WeaponItemSo weaponItem)
        {
            var available = new List<WeaponModificationConfig>();

            foreach (var modification in _catalog.Modifications)
            {
                if (modification != null && weaponItem.HasSocket(modification.SocketType))
                    available.Add(modification);
            }

            return available;
        }
    }
}
