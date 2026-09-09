using UnityEngine;

namespace VFXModule
{
    public interface IEffectHandle
    {
        void Stop();
    }

    public interface IEffectsService
    {
        IEffectHandle Fire(EffectId effectId, Vector3 position, Quaternion rotation, Transform parent = null, Vector3? scale = null);
    }
}
