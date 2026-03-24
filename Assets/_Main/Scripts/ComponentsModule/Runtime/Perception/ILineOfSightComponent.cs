using UnityEngine;

namespace ComponentsModule
{
    public interface ILineOfSightComponent
    {
        bool CheckLineOfSight(Transform target, float range, float angle, LayerMask mask, out Vector3 hitPoint);
    }
}