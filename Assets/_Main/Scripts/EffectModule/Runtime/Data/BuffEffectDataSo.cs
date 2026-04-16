using UnityEngine;

namespace EffectModule
{
    public abstract class BuffEffectDataSo : ScriptableObject
    {
        public abstract IBuff CreateBuff(float duration);
    }
}
