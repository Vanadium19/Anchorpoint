using UnityEngine;
using ComponentsModule;

namespace EffectModule
{
    public interface IBuffTarget
    {
        bool TryGet<T>(out T component) where T : class;
        Transform Transform { get; }
    }
}
