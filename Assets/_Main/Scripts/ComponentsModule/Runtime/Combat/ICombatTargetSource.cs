using UnityEngine;

namespace ComponentsModule
{
    public interface ICombatTargetSource
    {
        bool TryGetNearest(Vector3 origin, float radius, out ICombatTarget target);
    }
}
