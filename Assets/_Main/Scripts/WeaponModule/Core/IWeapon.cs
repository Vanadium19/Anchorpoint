using Cysharp.Threading.Tasks; // Добавь using

namespace WeaponModule.Core
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