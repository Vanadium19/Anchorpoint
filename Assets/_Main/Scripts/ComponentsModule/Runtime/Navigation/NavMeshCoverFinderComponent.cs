using UnityEngine;
using UnityEngine.AI;

namespace ComponentsModule
{
    public class NavMeshCoverFinderComponent : ICoverFinderComponent
    {
        private const int SearchAttempts = 10;
        private const float NavMeshSampleDistance = 2f;

        private readonly Transform _transform;

        public NavMeshCoverFinderComponent(Transform transform)
        {
            _transform = transform;
        }

        public bool TryFindCover(Vector3 threatPosition, float searchRadius, LayerMask obstructionMask, out Vector3 coverPosition)
        {
            for (int i = 0; i < SearchAttempts; i++)
            {
                var randomPoint = _transform.position + Random.insideUnitSphere * searchRadius;

                if (!NavMesh.SamplePosition(randomPoint, out var hit, NavMeshSampleDistance, NavMesh.AllAreas))
                    continue;

                var direction = hit.position - threatPosition;
                var distance = direction.magnitude;

                if (distance <= Mathf.Epsilon)
                    continue;

                if (!Physics.Raycast(threatPosition + Vector3.up, direction.normalized, out var rayHit, distance, obstructionMask))
                    continue;

                if (rayHit.transform.root == _transform.root)
                    continue;

                coverPosition = hit.position;
                return true;
            }

            coverPosition = _transform.position;
            return false;
        }
    }
}
