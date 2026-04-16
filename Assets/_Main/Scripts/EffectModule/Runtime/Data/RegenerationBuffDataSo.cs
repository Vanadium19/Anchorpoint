using UnityEngine;

namespace EffectModule
{
    [CreateAssetMenu(fileName = "RegenerationBuff", menuName = "Effects/RegenerationBuff")]
    public class RegenerationBuffDataSo : BuffEffectDataSo
    {
        [SerializeField] private float healPerTick = 5f;
        [SerializeField] private float tickInterval = 1f;

        public float HealPerTick => healPerTick;
        public float TickInterval => tickInterval;

        public override IBuff CreateBuff(float duration)
        {
            return new RegenerationBuff(duration, healPerTick, tickInterval);
        }
    }
}
