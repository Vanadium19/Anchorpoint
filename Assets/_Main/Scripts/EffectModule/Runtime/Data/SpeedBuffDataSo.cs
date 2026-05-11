using UnityEngine;

namespace EffectModule
{
    [CreateAssetMenu(fileName = "SpeedBuff", menuName = "Effects/SpeedBuff")]
    public class SpeedBuffDataSo : BuffEffectDataSo
    {
        [SerializeField] private float speedMultiplier = 1.5f;

        public float SpeedMultiplier => speedMultiplier;

        public override IBuff CreateBuff(float duration)
        {
            return new SpeedBuff(duration, speedMultiplier);
        }
    }
}
