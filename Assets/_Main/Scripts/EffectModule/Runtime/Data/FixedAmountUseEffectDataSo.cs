using ComponentsModule;
using InventoryModule;
using System.Collections.Generic;
using UnityEngine;
using WeaponModule;

namespace EffectModule
{
    [CreateAssetMenu(fileName = "FixedAmountUseEffect", menuName = "Effects/FixedAmount Use Effect")]
    public class FixedAmountUseEffectDataSo : UseEffectDataSo, IUseEffectData
    {
        [SerializeField] private int durabilityPerUse = 1;
        [SerializeField] private List<EffectDataSo> effects = new();
        [SerializeField] private List<BuffDataSo> buffs = new();

        public override int DurabilityPerUse => durabilityPerUse;
        public IReadOnlyList<EffectDataSo> Effects => effects;
        public IReadOnlyList<BuffDataSo> Buffs => buffs;

        public override bool CanApply(object target)
        {
            var entity = target as IEntity;

            if (entity == null)
                return false;

            if (HasApplicableEffect(entity))
                return true;

            return buffs != null && buffs.Count > 0;
        }

        private bool HasApplicableEffect(IEntity entity)
        {
            for (var i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];

                if (effect == null)
                    continue;

                if (!(effect is AmmoEffectDataSo))
                    return true;

                if (!entity.TryGet<IWeaponInventory>(out var weaponInventory))
                    continue;

                var currentWeapon = weaponInventory.CurrentWeapon;

                if (currentWeapon != null && currentWeapon.Model.GetReserveSpace() > 0)
                    return true;
            }

            return false;
        }
    }
}
