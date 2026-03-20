using ComponentsModule;
using FSMModule;
using PlayerModule;
using UnityEngine;

namespace EnemyModule
{
    public sealed class AttackState : IState
    {
        private readonly EnemyConfig _config;
        private readonly EnemyView _view;
        private readonly PlayerProvider _player;
        private readonly IRangedAttackComponent _attack;
        private readonly Blackboard _blackboard;
        private readonly IPathMoveComponent _movement;
        private readonly ITargetRotationComponent _rotation;
        private readonly ILineOfSightComponent _lineOfSight;

        private Transform _playerTransform;
        private float _nextFireTime;

        public AttackState(
            EnemyConfig config,
            EnemyView view,
            PlayerProvider player,
            IRangedAttackComponent attack,
            Blackboard blackboard,
            IPathMoveComponent movement,
            ITargetRotationComponent rotation,
            ILineOfSightComponent lineOfSight)
        {
            _config = config;
            _view = view;
            _player = player;
            _attack = attack;
            _blackboard = blackboard;
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

            if (!_blackboard.TryGetValue(BlackboardTag.TargetPosition, out Vector3 targetPosition))
                return;

            _rotation.RotateTowards(targetPosition, _config.CombatTurnSpeed);

            if (Time.time < _nextFireTime)
                return;

            if (!_attack.TryAttack(targetPosition, _config.Damage, _config.BulletSpeed))
                return;

            float fireRate = Mathf.Max(_config.FireRate, 0.01f);
            _nextFireTime = Time.time + (1f / fireRate);
        }

        public void OnExit()
        {
            _movement.Stop();
        }

        public bool NeedsReload => _attack.IsEmpty;

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

            _blackboard.SetValue(BlackboardTag.TargetPosition, targetPosition);
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
