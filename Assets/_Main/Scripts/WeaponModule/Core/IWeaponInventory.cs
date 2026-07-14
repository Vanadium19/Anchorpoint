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
    }
}
