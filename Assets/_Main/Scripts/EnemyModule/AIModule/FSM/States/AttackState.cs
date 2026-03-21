using ComponentsModule;
using FSMModule;
using PlayerModule;
using UnityEngine;

namespace EnemyModule
{
    public sealed class AttackState : IState
    {
        private readonly EnemyConfig _config;

        private readonly Transform _transform;

        private readonly PlayerProvider _playerProvider;
        private readonly Blackboard _blackboard;

        private readonly IRangedAttackComponent _attack;
        private readonly IPathMoveComponent _movement;
        private readonly ITargetRotationComponent _rotation;
        private readonly ILineOfSightComponent _lineOfSight;

        private Transform _player;
        private float _nextFireTime;

        public AttackState(EnemyConfig config,
            Transform transform,
            PlayerProvider playerProvider,
            IRangedAttackComponent attack,
            Blackboard blackboard,
            IPathMoveComponent movement,
            ITargetRotationComponent rotation,
            ILineOfSightComponent lineOfSight)
        {
            _config = config;
            _transform = transform;
            _playerProvider = playerProvider;
            _attack = attack;
            _blackboard = blackboard;
            _movement = movement;
            _rotation = rotation;
            _lineOfSight = lineOfSight;
        }

        public bool NeedsReload => _attack.IsEmpty;

        public void OnEnter()
        {
            _player ??= _playerProvider.Get<Transform>();
            _movement.Stop();
        }

        public void OnUpdate(float deltaTime)
        {
            TryUpdateVisibleTarget(out _);

            _movement.Stop();

            if (!_blackboard.TryGetValue(BlackboardTag.TargetPosition, out Vector3 targetPosition))
                return;

            _rotation.RotateTowards(targetPosition, _config.CombatTurnSpeed);

            if (Time.time < _nextFireTime)
                return;

            if (!_attack.TryAttack(targetPosition, _config.Damage, _config.BulletSpeed))
                return;

            var fireRate = Mathf.Max(_config.FireRate, 0.01f);
            _nextFireTime = Time.time + (1f / fireRate);
        }

        public void OnExit() => _movement.Stop();

        public bool ShouldExitAttack()
        {
            if (!TryUpdateVisibleTarget(out var targetPosition))
                return true;

            var maxAttackDistance = _config.AttackRange * _config.AttackExitRangeMultiplier;
            return Vector3.Distance(_transform.position, targetPosition) > maxAttackDistance;
        }

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
            return true;
        }
    }
}