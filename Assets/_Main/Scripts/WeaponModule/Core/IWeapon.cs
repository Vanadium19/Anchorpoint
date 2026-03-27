using Cysharp.Threading.Tasks;

namespace WeaponModule
{
    public interface IWeapon
    {
        public WeaponModel Model { get; }

        void Initialize();
        void Equip();
        void Hide();
        UniTask Unequip();
    }
}