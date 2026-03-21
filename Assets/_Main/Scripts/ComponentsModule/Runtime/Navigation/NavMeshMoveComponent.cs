using UnityEngine;
using UnityEngine.AI;

namespace ComponentsModule
{
    public class NavMeshMoveComponent : IPathMoveComponent
    {
        private const float MinMovementSqrMagnitude = 0.01f;

        private readonly NavMeshAgent _agent;

        public NavMeshMoveComponent(NavMeshAgent agent)
        {
            _agent = agent;
        }

        public bool IsMoving => _agent.velocity.sqrMagnitude > MinMovementSqrMagnitude;
        public bool IsPathPending => _agent.pathPending;
        public float RemainingDistance => _agent.remainingDistance;
        public float StoppingDistance => _agent.stoppingDistance;
        public Vector3 Velocity => _agent.velocity;

        public void SetStoppingDistance(float value) => _agent.stoppingDistance = value;

        public void MoveTo(Vector3 position)
        {
            if (!IsAgentReady())
                return;

            _agent.isStopped = false;
            _agent.SetDestination(position);
        }

        public void Stop()
        {
            if (!IsAgentReady())
                return;

            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;
        }

        public void ResetPath()
        {
            if (!IsAgentReady())
                return;

            _agent.ResetPath();
        }

        private bool IsAgentReady() => _agent.enabled && _agent.isOnNavMesh;
    }
}