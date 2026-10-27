using ComponentsModule;
using UnityEngine;

namespace EffectModule
{
    public abstract class EffectDataSo : ScriptableObject
    {
        public virtual bool CanApply(IEntity target) => true;

        public abstract void Apply(IEntity target);
    }
}
