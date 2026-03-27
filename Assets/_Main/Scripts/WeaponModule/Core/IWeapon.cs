using Cysharp.Threading.Tasks;

namespace WeaponModule
{
    public interface IWeapon
    {
        void Initialize();
        void Equip();
        void Hide();
        UniTask Unequip();
    }
}