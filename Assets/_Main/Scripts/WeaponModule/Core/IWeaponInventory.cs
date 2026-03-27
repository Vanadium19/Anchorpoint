namespace WeaponModule
{
    public interface IWeaponInventory
    {
        IWeapon CurrentWeapon { get; }
        int WeaponsCount { get; }

        void Initialize();
        void EquipWeapon(int index);
        void UnequipCurrentWeapon();
        void EquipLastWeapon();
    }
}