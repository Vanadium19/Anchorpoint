namespace WeaponModule
{
    public class WeaponStatsProvider : IWeaponStatsProvider
    {
        public WeaponConfig GetConfig(WeaponItemSo weaponItem)
        {
            return weaponItem != null ? weaponItem.WeaponConfig : null;
        }
    }
}
