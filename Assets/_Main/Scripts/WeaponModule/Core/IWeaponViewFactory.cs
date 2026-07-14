namespace WeaponModule
{
    public interface IWeaponViewFactory
    {
        WeaponView CreateView(WeaponItemSo weaponItem);
    }
}
