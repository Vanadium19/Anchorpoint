using ComponentsModule;
using UnityEngine;
using WeaponModule;

namespace EffectModule
{
    [CreateAssetMenu(fileName = "AmmoEffect", menuName = "Effects/Ammo")]
    public class AmmoEffectDataSo : EffectDataSo
    {
        [SerializeField] private int amount = 1;
        [SerializeField] private AmmoType ammoType = AmmoType.Rifle;

        public int Amount => amount;
        public AmmoType AmmoType => ammoType;

        public override bool CanApply(IEntity target)
        {
            return target.TryGet<IWeaponInventory>(out var weaponInventory)
                && weaponInventory.CanAcceptAmmo(ammoType);
        }

        public override void Apply(IEntity target)
        {
            if (!target.TryGet<IWeaponInventory>(out var weaponInventory))
                return;

            weaponInventory.TryAddReserveAmmo(ammoType, amount);
        }
    }
}
