using System.Collections.Generic;
using UnityEngine;
using InventoryModule;

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
            return true;
        }
    }
}
