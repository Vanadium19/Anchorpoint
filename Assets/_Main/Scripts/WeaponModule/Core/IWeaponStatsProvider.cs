namespace WeaponModule
{
    public interface IWeaponStatsProvider
    {
        WeaponConfig GetConfig(WeaponItemSo weaponItem);
    }
}
