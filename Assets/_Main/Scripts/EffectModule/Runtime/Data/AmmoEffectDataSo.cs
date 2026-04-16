using UnityEngine;
using InventoryModule;
using WeaponModule;

namespace EffectModule
{
    [CreateAssetMenu(fileName = "AmmoEffect", menuName = "Effects/Ammo")]
    public class AmmoEffectDataSo : EffectDataSo
    {
        [SerializeField] private int amount = 1;

        public int Amount => amount;

        public override void Apply(IEffectTarget target)
        {
            if (!target.TryGet<IWeaponInventory>(out var weaponInventory))
                return;

            var currentWeapon = weaponInventory.CurrentWeapon;

            if (currentWeapon == null)
                return;

            currentWeapon.Model.AddReserveAmmo(amount);
        }
    }
}
