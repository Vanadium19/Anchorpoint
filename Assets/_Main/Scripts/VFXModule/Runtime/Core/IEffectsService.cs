using UnityEngine;

namespace VFXModule
{
    public interface IEffectsService
    {
        void Fire(EffectId effectId, Vector3 position, Quaternion rotation, Transform parent = null);
    }
}