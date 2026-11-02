using InventoryModule;

namespace WeaponModule
{
    public interface IWeaponInventory
    {
        IWeapon CurrentWeapon { get; }
        int WeaponsCount { get; }

        void Initialize();
        void EquipWeapon(ItemTable item);
        void UnequipCurrentWeapon();
        bool CanAcceptAmmo(AmmoType ammoType);
        bool TryAddReserveAmmo(AmmoType ammoType, int amount);

        /// <summary>
        /// Re-applies the build (modifications from the item metadata) to the weapon created for the item.
        /// Does nothing if no weapon was created for the item yet; it gets the build when it is created.
        /// </summary>
        void RefreshBuild(ItemTable item);
    }
}
