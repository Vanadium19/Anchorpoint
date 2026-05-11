using UnityEngine;

namespace InventoryModule
{
    public abstract class UseEffectDataSo : ScriptableObject
    {
        public abstract int DurabilityPerUse { get; }
        public abstract bool CanApply(object target);
    }
}
