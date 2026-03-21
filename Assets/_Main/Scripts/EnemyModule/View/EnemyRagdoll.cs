using UnityEngine;
using UnityEngine.AI;

namespace EnemyModule
{
    public class EnemyRagdoll : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private Collider mainCollider;

        private Rigidbody[] _bodies;
        private Collider[] _colliders;

        private void Awake()
        {
            _bodies = GetComponentsInChildren<Rigidbody>();
            _colliders = GetComponentsInChildren<Collider>();
            ToggleRagdoll(false);
        }

        public void Activate(Vector3? force = null, Vector3? hitPoint = null)
        {
            if (animator)
                animator.enabled = false;

            if (agent)
                agent.enabled = false;

            if (mainCollider)
                mainCollider.enabled = false;

            ToggleRagdoll(true);

            if (force.HasValue && hitPoint.HasValue)
                ApplyForce(hitPoint.Value, force.Value);
        }

        private void ToggleRagdoll(bool state)
        {
            foreach (var body in _bodies)
                body.isKinematic = !state;

            foreach (var collider in _colliders)
            {
                if (collider != mainCollider)
                {
                    collider.enabled = state;
                }
            }
        }

        private void ApplyForce(Vector3 position, Vector3 direction)
        {
            var closest = default(Rigidbody);
            var minDistance = float.MaxValue;

            foreach (var body in _bodies)
            {
                var distance = Vector3.Distance(body.position, position);

                if ((distance >= minDistance))
                    continue;

                minDistance = distance;
                closest = body;
            }

            if (closest)
                closest.AddForce(direction, ForceMode.Impulse);
        }
    }
}