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
        [SerializeField] private List<WeaponModificationConfig> availableModifications = new();

        public WeaponConfig WeaponConfig => weaponConfig;
        public WeaponView WeaponViewPrefab => weaponViewPrefab;
        public IReadOnlyList<WeaponModificationConfig> AvailableModifications => availableModifications;

        public bool TryGetModification(string modificationID, out WeaponModificationConfig modification)
        {
            foreach (var candidate in availableModifications)
            {
                if (candidate != null && candidate.ID == modificationID)
                {
                    modification = candidate;
                    return true;
                }
            }

            modification = null;
            return false;
        }
    }
}
