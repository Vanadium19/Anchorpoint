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

        /// <summary>
        /// Checks whether the view of this weapon has a socket of the given type. This is the compatibility rule
        /// for modifications: the view prefab is the only place that defines it.
        /// </summary>
        /// <param name="socketType">The socket type a modification needs.</param>
        /// <returns>True if the view has such a socket.</returns>
        public bool HasSocket(WeaponSocketType socketType)
        {
            if (socketType == null || weaponViewPrefab == null)
                return false;

            var modificationView = weaponViewPrefab.ModificationView;

            if (modificationView == null)
                return false;

            var sockets = modificationView.Sockets;

            for (var i = 0; i < sockets.Count; i++)
            {
                if (sockets[i] != null && sockets[i].Type == socketType)
                    return true;
            }

            return false;
        }
    }
}
