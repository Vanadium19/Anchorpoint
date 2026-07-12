namespace InventoryModule
{
    [System.Flags]
    public enum EquipmentSlotType
    {
        None = 0,
        Head = 1,
        Chest = 2,
        Backpack = 4,
        Pockets = 8,
        PrimaryWeapon1 = 16,
        PrimaryWeapon2 = 64,
        WeaponSecondary = 32,
    }
}