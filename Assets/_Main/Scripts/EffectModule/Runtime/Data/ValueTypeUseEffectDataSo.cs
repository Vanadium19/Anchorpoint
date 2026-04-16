using System.Collections.Generic;
using UnityEngine;
using InventoryModule;
using ComponentsModule;

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
            var effectTarget = target as IEffectTarget;

            if (effectTarget == null)
                return false;

            foreach (var effect in effects)
            {
                if (effect is HealEffectDataSo)
                {
                    if (effectTarget.TryGet<IHealthComponent>(out var health))
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
