using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ComponentsModule
{
    public class NavMeshMoveComponent : IPathMoveComponent
    {
        private const float MinMovementSqrMagnitude = 0.01f;

        private readonly NavMeshAgent _agent;
        private readonly Dictionary<object, float> _speedMultipliers = new();
        private readonly float _baseSpeed;

        public NavMeshMoveComponent(NavMeshAgent agent)
        {
            _agent = agent;
            _baseSpeed = agent.speed;
        }

        public bool IsMoving => _agent.velocity.sqrMagnitude > MinMovementSqrMagnitude;
        public bool IsPathPending => _agent.pathPending;
        public float RemainingDistance => _agent.remainingDistance;
        public float StoppingDistance => _agent.stoppingDistance;
        public Vector3 Velocity => _agent.velocity;
        public float BaseSpeed => _baseSpeed;
        public float CurrentSpeed => _agent != null ? _agent.speed : 0f;

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

        public void ApplySpeedMultiplier(object source, float multiplier)
        {
            if (source == null)
                return;

            _speedMultipliers[source] = Mathf.Clamp(multiplier, 0.01f, 1f);
            RefreshSpeed();
        }

        public void RemoveSpeedMultiplier(object source)
        {
            if (source == null)
                return;

            _speedMultipliers.Remove(source);
            RefreshSpeed();
        }

        private bool IsAgentReady() => _agent != null && _agent.enabled && _agent.isOnNavMesh;

        private void RefreshSpeed()
        {
            if (_agent == null)
                return;

            var speedMultiplier = 1f;

            foreach (var multiplier in _speedMultipliers.Values)
                speedMultiplier = Mathf.Min(speedMultiplier, multiplier);

            _agent.speed = _baseSpeed * speedMultiplier;
        }
    }
}
