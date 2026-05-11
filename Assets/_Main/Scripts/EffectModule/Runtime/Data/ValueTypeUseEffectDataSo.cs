using ComponentsModule;
using System.Collections.Generic;
using UnityEngine;
using InventoryModule;

namespace EffectModule
{
    [CreateAssetMenu(fileName = "ValueTypeUseEffect", menuName = "Effects/ValueType Use Effect")]
    public class ValueTypeUseEffectDataSo : UseEffectDataSo, IUseEffectData
    {
        [SerializeField] private int durabilityPerUse = 1;
        [SerializeField] private List<EffectDataSo> effects = new();

        public override int DurabilityPerUse => durabilityPerUse;
        public IReadOnlyList<EffectDataSo> Effects => effects;
        public IReadOnlyList<BuffDataSo> Buffs => null;

        public override bool CanApply(object target)
        {
            var entity = target as IEntity;

            if (entity == null)
                return false;

            foreach (var effect in effects)
            {
                if (effect is HealEffectDataSo)
                {
                    if (entity.TryGet<IHealthComponent>(out var health))
                    {
                        if (health.CurrentHealth >= health.MaxHealth)
                            return false;
                    }
                }
            }

            return true;
        }
    }
}
