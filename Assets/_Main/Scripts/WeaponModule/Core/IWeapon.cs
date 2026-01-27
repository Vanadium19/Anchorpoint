using Cysharp.Threading.Tasks;

namespace WeaponModule
{
    public interface IWeapon
    {
        void Initialize();
        void Tick();
        void LateTick();
        void Equip();
        UniTask Unequip();
    }
}