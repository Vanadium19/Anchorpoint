using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace NpcBaseModule
{
    public sealed class FriendlyNpcController : ITickable, IInitializable
    {
        private const float MinRemainingDistance = 0.1f;
        private const float RetryWaitSeconds = 0.5f;
        private const int NavMeshSampleAttempts = 30;
        private const float MinDirectionSqrMagnitude = 0.0001f;

        private readonly FriendlyNpcView _view;

        private FriendlyNpcState _state;
        private Transform _player;

        private float _waitTimer;
        private Vector3 _currentDestination;
        private bool _hasDestination;

        public FriendlyNpcController(FriendlyNpcView view)
        {
            _view = view;
            _state = FriendlyNpcState.Patrolling;
        }

        public void Initialize()
        {
            _view.SetController(this);

            ApplyAgentSettings();
            HideText();
            StartPatrol(forceNewPoint: true);
        }

        public void OnInteract(Transform player)
        {
            if (player == null)
                return;

            if (_state == FriendlyNpcState.Responding)
                return;

            _player = player;
            EnterResponding();
        }

        public void Tick()
        {
            if (_view.Agent == null)
                return;

            switch (_state)
            {
                case FriendlyNpcState.Patrolling:
                    TickPatrolling();
                    return;

                case FriendlyNpcState.Responding:
                    TickResponding();
                    return;

                default:
                    return;
            }
        }

        private void TickPatrolling()
        {
            NavMeshAgent agent = _view.Agent;

            if (!_hasDestination)
            {
                if (_waitTimer > 0f)
                {
                    _waitTimer -= Time.deltaTime;
                    return;
                }

                TrySetNewDestination();
                return;
            }

            if (agent.pathPending)
                return;

            float remainingDistanceThreshold = Mathf.Max(agent.stoppingDistance, MinRemainingDistance);

            if (agent.remainingDistance <= remainingDistanceThreshold)
            {
                _hasDestination = false;
                _waitTimer = Random.Range(_view.Settings.MinWait, _view.Settings.MaxWait);
            }
        }

        private void TickResponding()
        {
            if (_player == null)
            {
                ExitResponding();
                return;
            }

            float distance = Vector3.Distance(_player.position, _view.Transform.position);

            if (distance > _view.Settings.ExitRadius)
            {
                ExitResponding();
                return;
            }

            FaceTarget(_player.position);
        }

        private void EnterResponding()
        {
            _state = FriendlyNpcState.Responding;

            NavMeshAgent agent = _view.Agent;
            agent.isStopped = true;
            agent.ResetPath();

            ShowText();
        }

        private void ExitResponding()
        {
            _player = null;
            HideText();

            NavMeshAgent agent = _view.Agent;
            agent.isStopped = false;

            _state = FriendlyNpcState.Patrolling;
            StartPatrol(forceNewPoint: true);
        }

        private void StartPatrol(bool forceNewPoint)
        {
            _hasDestination = false;
            _waitTimer = 0f;

            if (forceNewPoint)
                TrySetNewDestination();
        }

        private void TrySetNewDestination()
        {
            NavMeshAgent agent = _view.Agent;

            if (TryGetRandomNavMeshPoint(_view.Transform.position, _view.Settings.PatrolRadius, out Vector3 point))
            {
                _currentDestination = point;
                _hasDestination = true;
                agent.SetDestination(_currentDestination);
                return;
            }

            _hasDestination = false;
            _waitTimer = RetryWaitSeconds;
        }

        private bool TryGetRandomNavMeshPoint(Vector3 origin, float radius, out Vector3 result)
        {
            for (int attemptIndex = 0; attemptIndex < NavMeshSampleAttempts; attemptIndex++)
            {
                Vector3 randomPoint = origin + Random.insideUnitSphere * radius;

                if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, radius, NavMesh.AllAreas))
                {
                    result = hit.position;
                    return true;
                }
            }

            result = origin;
            return false;
        }

        private void FaceTarget(Vector3 targetPosition)
        {
            Vector3 selfPosition = _view.Transform.position;
            Vector3 direction = targetPosition - selfPosition;
            direction.y = 0f;

            if (direction.sqrMagnitude < MinDirectionSqrMagnitude)
                return;

            float targetYaw = Quaternion.LookRotation(direction, Vector3.up).eulerAngles.y;
            float currentYaw = _view.Transform.eulerAngles.y;

            float newYaw = Mathf.MoveTowardsAngle(currentYaw, targetYaw, _view.Settings.TurnSpeed * Time.deltaTime);
            _view.Transform.rotation = Quaternion.Euler(0f, newYaw, 0f);
        }

        private void ApplyAgentSettings()
        {
            NavMeshAgent agent = _view.Agent;

            if (agent == null)
                return;

            agent.speed = _view.Settings.MoveSpeed;
        }

        private void ShowText() => _view.WorldText?.Show(_view.Settings.MessageText);

        private void HideText() => _view.WorldText?.Hide();
    }
}