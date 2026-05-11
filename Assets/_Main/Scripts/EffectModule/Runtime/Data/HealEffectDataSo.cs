using ComponentsModule;
using UnityEngine;

namespace EffectModule
{
    [CreateAssetMenu(fileName = "HealEffect", menuName = "Effects/Heal")]
    public class HealEffectDataSo : EffectDataSo
    {
        [SerializeField] private int amount = 1;

        public int Amount => amount;

        public override void Apply(IEntity target)
        {
            if (target.TryGet<IHealthComponent>(out var health))
                health.Heal(amount);
        }
    }
}
