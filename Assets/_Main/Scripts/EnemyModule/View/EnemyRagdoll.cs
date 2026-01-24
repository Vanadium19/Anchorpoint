using UnityEngine;
using UnityEngine.AI;

namespace EnemyModule.View
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
            if (animator) animator.enabled = false;
            if (agent) agent.enabled = false;
            if (mainCollider) mainCollider.enabled = false;

            ToggleRagdoll(true);

            if (force.HasValue && hitPoint.HasValue)
            {
                ApplyForce(hitPoint.Value, force.Value);
            }
        }

        private void ToggleRagdoll(bool state)
        {
            foreach (var rb in _bodies) rb.isKinematic = !state;
            foreach (var col in _colliders)
            {
                if (col != mainCollider) col.enabled = state;
            }
        }

        private void ApplyForce(Vector3 pos, Vector3 dir)
        {
            Rigidbody closest = null;
            float minDst = float.MaxValue;
            foreach (var rb in _bodies)
            {
                float d = Vector3.Distance(rb.position, pos);
                if (d < minDst) { minDst = d; closest = rb; }
            }
            if (closest) closest.AddForce(dir, ForceMode.Impulse);
        }
    }
}