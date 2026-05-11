using ComponentsModule;
using UnityEngine;

namespace EffectModule
{
    public abstract class EffectDataSo : ScriptableObject
    {
        public abstract void Apply(IEntity target);
    }
}
