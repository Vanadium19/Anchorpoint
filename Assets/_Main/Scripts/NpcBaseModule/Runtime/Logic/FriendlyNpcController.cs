using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace NpcModule.Runtime
{
    public sealed class FriendlyNpcController : ITickable, IInitializable
    {
        private readonly FriendlyNpcView _view;

        private FriendlyNpcState _state;
        private Transform _player;

        private float _waitTimer;
        private Vector3 _currentDestination;
        private bool _hasDestination;

        public void Initialize()
        {
            _view.SetController(this);

            ApplyAgentSettings();
            HideText();
            StartPatrol(forceNewPoint: true);
        }

        public FriendlyNpcController(FriendlyNpcView view)
        {
            _view = view;
            _state = FriendlyNpcState.Patrolling;
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
            if (_view == null || _view.Transform == null)
                return;

            if (_view.Agent == null)
                return;

            switch (_state)
            {
                case FriendlyNpcState.Patrolling:
                    TickPatrolling();
                    break;

                case FriendlyNpcState.Responding:
                    TickResponding();
                    break;
            }
        }

        private void TickPatrolling()
        {
            var agent = _view.Agent;

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

            if (!agent.pathPending && agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, 0.1f))
            {
                _hasDestination = false;
                _waitTimer = Random.Range(_view.Settings.minWait, _view.Settings.maxWait);
            }
        }

        private void TickResponding()
        {
            if (_player == null)
            {
                ExitResponding();
                return;
            }

            float dist = Vector3.Distance(_player.position, _view.Transform.position);
            if (dist > _view.Settings.exitRadius)
            {
                ExitResponding();
                return;
            }

            FaceTarget(_player.position);
        }

        private void EnterResponding()
        {
            _state = FriendlyNpcState.Responding;

            var agent = _view.Agent;
            agent.isStopped = true;
            agent.ResetPath();

            ShowText();
        }

        private void ExitResponding()
        {
            _player = null;
            HideText();

            var agent = _view.Agent;
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
            var agent = _view.Agent;

            if (TryGetRandomNavMeshPoint(_view.Transform.position, _view.Settings.patrolRadius, out var point))
            {
                _currentDestination = point;
                _hasDestination = true;
                agent.SetDestination(_currentDestination);
            }
            else
            {
                _hasDestination = false;
                _waitTimer = 0.5f;
            }


        }

        private static bool TryGetRandomNavMeshPoint(Vector3 origin, float radius, out Vector3 result)
        {
            for (int i = 0; i < 30; i++)
            {
                Vector3 random = origin + Random.insideUnitSphere * radius;
                if (NavMesh.SamplePosition(random, out var hit, radius, NavMesh.AllAreas))
                {
                    result = hit.position;
                    return true;
                }
            }

            result = origin;
            return false;
        }


        private void FaceTarget(Vector3 targetPos)
        {
            var selfPos = _view.Transform.position;
            Vector3 dir = targetPos - selfPos;
            dir.y = 0f;

            if (dir.sqrMagnitude < 0.0001f)
                return;

            float targetYaw = Quaternion.LookRotation(dir, Vector3.up).eulerAngles.y;
            float currentYaw = _view.Transform.eulerAngles.y;

            float newYaw = Mathf.MoveTowardsAngle(currentYaw, targetYaw, _view.Settings.turnSpeed * Time.deltaTime);
            _view.Transform.rotation = Quaternion.Euler(0f, newYaw, 0f);
        }

        private void ApplyAgentSettings()
        {
            var agent = _view.Agent;
            if (agent == null)
                return;

            agent.speed = _view.Settings.moveSpeed;
        }

        private void ShowText()
        {
            if (_view.WorldText == null)
                return;

            _view.WorldText.Show(_view.Settings.messageText);
        }

        private void HideText()
        {
            if (_view.WorldText == null)
                return;

            _view.WorldText.Hide();
        }
    }
}
