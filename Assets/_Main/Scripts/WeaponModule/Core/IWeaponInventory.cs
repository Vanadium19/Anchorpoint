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
    }
}
