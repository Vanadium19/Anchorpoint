using System.Collections.Generic;
using InventoryModule;
using UnityEngine;

namespace WeaponModule
{
    [CreateAssetMenu(fileName = "WeaponItem", menuName = "Inventory/Items/Weapon")]
    public class WeaponItemSo : ItemDataSo
    {
        [SerializeField] private WeaponConfig weaponConfig;
        [SerializeField] private WeaponView weaponViewPrefab;
        [SerializeField] private WeaponModificationConfig[] availableModifications;

        public WeaponConfig WeaponConfig => weaponConfig;
        public WeaponView WeaponViewPrefab => weaponViewPrefab;

        /// <summary>
        /// Modifications that can be installed on this weapon. This list is the compatibility rule.
        /// </summary>
        public IReadOnlyList<WeaponModificationConfig> AvailableModifications =>
            availableModifications ?? System.Array.Empty<WeaponModificationConfig>();
    }
}
