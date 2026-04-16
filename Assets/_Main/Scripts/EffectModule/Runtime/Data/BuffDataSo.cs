using System.Collections.Generic;
using UnityEngine;

namespace EffectModule
{
    [CreateAssetMenu(fileName = "BuffData", menuName = "Effects/BuffData")]
    public class BuffDataSo : ScriptableObject
    {
        [SerializeField] private float duration = 10f;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite icon;
        [SerializeField] private List<BuffEffectDataSo> buffEffects = new();

        public float Duration => duration;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public IReadOnlyList<BuffEffectDataSo> BuffEffects => buffEffects;

        public void Apply(IBuffTarget target, IBuffService buffService)
        {
            if (target == null || buffService == null)
                return;

            foreach (var buffEffect in buffEffects)
            {
                var buff = buffEffect.CreateBuff(duration);
                buffService.AddBuff(target, buff, this);
            }
        }
    }
}
