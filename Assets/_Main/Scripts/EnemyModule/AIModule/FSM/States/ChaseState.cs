using ComponentsModule;
using FSMModule;
using PlayerModule;
using UnityEngine;

namespace EnemyModule
{
    public sealed class ChaseState : IState
    {
        private readonly EnemyConfig _config;
        private readonly Transform _transform;

        private readonly PlayerProvider _playerProvider;
        private readonly Blackboard _blackboard;

        private readonly IPathMoveComponent _movement;
        private readonly ILineOfSightComponent _lineOfSight;

        private Transform _player;
        private float _chaseTimer;

        public ChaseState(EnemyConfig config,
            Transform transform,
            PlayerProvider playerProvider,
            Blackboard blackboard,
            IPathMoveComponent movement,
            ILineOfSightComponent lineOfSight)
        {
            _config = config;
            _transform = transform;
            _playerProvider = playerProvider;
            _blackboard = blackboard;
            _movement = movement;
            _lineOfSight = lineOfSight;
        }

        public void OnEnter()
        {
            _player ??= _playerProvider.Get<Transform>();
            _chaseTimer = _config.MemoryTime;

            if (_blackboard.TryGetValue(BlackboardTag.TargetPosition, out Vector3 targetPosition))
                _movement.MoveTo(targetPosition);
            else
                _movement.Stop();
        }

        public void OnUpdate(float deltaTime)
        {
            var canSeePlayer = TryUpdateVisibleTarget(out _);

            if (_blackboard.TryGetValue(BlackboardTag.TargetPosition, out Vector3 targetPosition))
                _movement.MoveTo(targetPosition);
            else
                _movement.Stop();

            if (canSeePlayer)
                return;

            if (!HasReachedLastKnownPosition())
                return;

            _chaseTimer -= deltaTime;
        }

        public void OnExit() { }

        public bool CanEnterAttack()
        {
            if (!TryUpdateVisibleTarget(out var targetPosition))
                return false;

            return Vector3.Distance(_transform.position, targetPosition) <= _config.AttackRange;
        }

        public bool ShouldReturnToPatrol() => _chaseTimer <= 0f && HasReachedLastKnownPosition();

        private bool TryUpdateVisibleTarget(out Vector3 targetPosition)
        {
            _player ??= _playerProvider.Get<Transform>();
            targetPosition = default;

            if (_player == null)
                return false;

            var canSeePlayer = _lineOfSight.CheckLineOfSight(_player, _config.SightDistance, _config.ViewAngle, _config.ViewMask, out targetPosition);

            if (!canSeePlayer)
                return false;

            _blackboard.SetValue(BlackboardTag.TargetPosition, targetPosition);
            _chaseTimer = _config.MemoryTime;
            return true;
        }

        private bool HasReachedLastKnownPosition()
        {
            if (!_blackboard.TryGetValue(BlackboardTag.TargetPosition, out Vector3 targetPosition))
                return true;

            return Vector3.Distance(_transform.position, targetPosition) <= _config.LostTargetReachDistance;
        }
    }
}