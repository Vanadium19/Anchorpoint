using ComponentsModule;
using FSMModule;
using PlayerModule;
using UnityEngine;

namespace EnemyModule
{
    public sealed class AttackState : IState
    {
        private readonly EnemyConfig _config;
        private readonly Enemy _enemy;
        private readonly EnemyView _view;
        private readonly PlayerProvider _player;
        private readonly IPathMoveComponent _movement;
        private readonly ITargetRotationComponent _rotation;
        private readonly ILineOfSightComponent _lineOfSight;

        private Transform _playerTransform;
        private float _nextFireTime;

        public AttackState(
            EnemyConfig config,
            Enemy enemy,
            EnemyView view,
            PlayerProvider player,
            IPathMoveComponent movement,
            ITargetRotationComponent rotation,
            ILineOfSightComponent lineOfSight)
        {
            _config = config;
            _enemy = enemy;
            _view = view;
            _player = player;
            _movement = movement;
            _rotation = rotation;
            _lineOfSight = lineOfSight;
        }

        public void OnEnter()
        {
            _playerTransform ??= ResolvePlayerTransform();
            _movement.Stop();
        }

        public void OnUpdate(float deltaTime)
        {
            TryUpdateVisibleTarget(out _);

            _movement.Stop();
            _rotation.RotateTowards(_enemy.LastKnownPosition, _config.CombatTurnSpeed);

            if (Time.time < _nextFireTime)
                return;

            float fireRate = Mathf.Max(_config.FireRate, 0.01f);
            _nextFireTime = Time.time + (1f / fireRate);

            if (_enemy.TryConsumeAmmo())
                _view.SpawnBullet(_enemy.LastKnownPosition, _config.Damage, _config.BulletSpeed);
        }

        public void OnExit()
        {
            _movement.Stop();
        }

        public bool NeedsReload => _enemy.CurrentAmmo <= 0;

        public bool ShouldExitAttack()
        {
            if (!TryUpdateVisibleTarget(out Vector3 targetPosition))
                return true;

            float maxAttackDistance = _config.AttackRange * _config.AttackExitRangeMultiplier;
            return Vector3.Distance(_view.transform.position, targetPosition) > maxAttackDistance;
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
            return true;
        }

        private Transform ResolvePlayerTransform()
        {
            if (_player.TryGet(out Transform playerTransform))
                return playerTransform;

            return null;
        }
    }
}
