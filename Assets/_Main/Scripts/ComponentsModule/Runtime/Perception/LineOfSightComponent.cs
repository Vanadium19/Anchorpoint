using UnityEngine;

namespace ComponentsModule
{
    public class LineOfSightComponent : ILineOfSightComponent
    {
        private const float DefaultTargetOffset = 1f;
        private const float HalfViewAngleFactor = 0.5f;

        private readonly Transform _eyes;

        public LineOfSightComponent(Transform eyes)
        {
            _eyes = eyes;
        }

        public bool CheckLineOfSight(Transform target, float range, float angle, LayerMask mask, out Vector3 hitPoint)
        {
            hitPoint = target != null ? target.position : default;

            if (!_eyes || !target)
                return false;

            var targetCenter = GetTargetCenter(target);
            var distance = Vector3.Distance(_eyes.position, targetCenter);

            if (distance > range)
                return false;

            var direction = (targetCenter - _eyes.position).normalized;
            var angleToTarget = Vector3.Angle(_eyes.forward, direction);

            if (angleToTarget > angle * HalfViewAngleFactor)
                return false;

            if (!Physics.Raycast(_eyes.position, direction, out var hit, range, mask))
                return false;

            if (hit.transform.root != target.root)
                return false;

            hitPoint = targetCenter;
            return true;
        }

        private static Vector3 GetTargetCenter(Transform target)
        {
            if (target.TryGetComponent(out Collider targetCollider))
                return targetCollider.bounds.center;

            return target.position + Vector3.up * DefaultTargetOffset;
        }
    }
}