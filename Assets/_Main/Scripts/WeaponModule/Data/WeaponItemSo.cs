using InventoryModule;
using UnityEngine;

namespace WeaponModule
{
    [CreateAssetMenu(fileName = "WeaponItem", menuName = "Inventory/Items/Weapon")]
    public class WeaponItemSo : ItemDataSo
    {
        [SerializeField] private WeaponConfig weaponConfig;
        [SerializeField] private WeaponView weaponViewPrefab;

        public WeaponConfig WeaponConfig => weaponConfig;
        public WeaponView WeaponViewPrefab => weaponViewPrefab;
    }
}
