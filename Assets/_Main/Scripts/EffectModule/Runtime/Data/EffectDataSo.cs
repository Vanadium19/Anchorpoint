using UnityEngine;
using InventoryModule;

namespace EffectModule
{
    public abstract class EffectDataSo : ScriptableObject
    {
        public abstract void Apply(IEffectTarget target);
    }
}
