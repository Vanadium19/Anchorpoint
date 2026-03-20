using ComponentsModule;
using FSMModule;
using PlayerModule;
using UnityEngine;

namespace EnemyModule
{
    public sealed class ChaseState : IState
    {
        private readonly EnemyConfig _config;
        private readonly Enemy _enemy;
        private readonly EnemyView _view;
        private readonly PlayerProvider _player;
        private readonly IPathMoveComponent _movement;
        private readonly ILineOfSightComponent _lineOfSight;

        private Transform _playerTransform;
        private float _chaseTimer;

        public ChaseState(
            EnemyConfig config,
            Enemy enemy,
            EnemyView view,
            PlayerProvider player,
            IPathMoveComponent movement,
            ILineOfSightComponent lineOfSight)
        {
            _config = config;
            _enemy = enemy;
            _view = view;
            _player = player;
            _movement = movement;
            _lineOfSight = lineOfSight;
        }

        public void OnEnter()
        {
            _playerTransform ??= ResolvePlayerTransform();
            _chaseTimer = _config.MemoryTime;
            _movement.MoveTo(_enemy.LastKnownPosition);
        }

        public void OnUpdate(float deltaTime)
        {
            bool canSeePlayer = TryUpdateVisibleTarget(out _);
            _movement.MoveTo(_enemy.LastKnownPosition);

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

            _enemy.SetTargetPosition(targetPosition);
            _chaseTimer = _config.MemoryTime;
            return true;
        }

        private bool HasReachedLastKnownPosition()
        {
            return Vector3.Distance(_view.transform.position, _enemy.LastKnownPosition) <= _config.LostTargetReachDistance;
        }

        private Transform ResolvePlayerTransform()
        {
            if (_player.TryGet(out Transform playerTransform))
                return playerTransform;

            return null;
        }
    }
}
