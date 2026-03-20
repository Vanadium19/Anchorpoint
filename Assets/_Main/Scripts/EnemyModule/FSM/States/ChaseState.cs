using ComponentsModule;
using FSMModule;
using PlayerModule;
using UnityEngine;

namespace EnemyModule
{
    public sealed class ChaseState : IState
    {
        private readonly EnemyConfig _config;
        private readonly EnemyView _view;
        private readonly PlayerProvider _player;
        private readonly Blackboard _blackboard;
        private readonly IPathMoveComponent _movement;
        private readonly ILineOfSightComponent _lineOfSight;

        private Transform _playerTransform;
        private float _chaseTimer;

        public ChaseState(
            EnemyConfig config,
            EnemyView view,
            PlayerProvider player,
            Blackboard blackboard,
            IPathMoveComponent movement,
            ILineOfSightComponent lineOfSight)
        {
            _config = config;
            _view = view;
            _player = player;
            _blackboard = blackboard;
            _movement = movement;
            _lineOfSight = lineOfSight;
        }

        public void OnEnter()
        {
            _playerTransform ??= ResolvePlayerTransform();
            _chaseTimer = _config.MemoryTime;

            if (_blackboard.TryGetValue(BlackboardTag.TargetPosition, out Vector3 targetPosition))
                _movement.MoveTo(targetPosition);
            else
                _movement.Stop();
        }

        public void OnUpdate(float deltaTime)
        {
            bool canSeePlayer = TryUpdateVisibleTarget(out _);

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

        public void OnExit()
        {
        }

        public bool CanEnterAttack()
        {
            if (!TryUpdateVisibleTarget(out Vector3 targetPosition))
                return false;

            return Vector3.Distance(_view.transform.position, targetPosition) <= _config.AttackRange;
        }

        public bool ShouldReturnToPatrol()
        {
            return _chaseTimer <= 0f && HasReachedLastKnownPosition();
        }

        private bool TryUpdateVisibleTarget(out Vector3 targetPosition)
        {
            _playerTransform ??= ResolvePlayerTransform();
            targetPosition = default;

            if (_playerTransform == null)
                return false;

            bool canSeePlayer = _lineOfSight.CheckLineOfSight(
                _playerTransform,
                _config.SightDistance,
                _config.ViewAngle,
                _config.ViewMask,
                out targetPosition);

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

            return Vector3.Distance(_view.transform.position, targetPosition) <= _config.LostTargetReachDistance;
        }

        private Transform ResolvePlayerTransform()
        {
            if (_player.TryGet(out Transform playerTransform))
                return playerTransform;

            return null;
        }
    }
}
