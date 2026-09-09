using ComponentsModule;
using FSMModule;
using PlayerModule;
using UnityEngine;
using Zenject;

namespace EnemyModule
{
    /// <summary>
    /// Walks to the nearest structure, shoots it with the enemy's weapon and reports when the player becomes visible.
    /// </summary>
    /// <remarks>
    /// The state reloads on the spot when the magazine runs dry, so damaging a structure never hands the enemy back to the shared reload state.
    /// </remarks>
    public sealed class AttackStructureState : IState
    {
        private readonly EnemyConfig _config;
        private readonly Transform _transform;

        private readonly PlayerProvider _playerProvider;
        private readonly Blackboard _blackboard;
        private readonly IRangedAttackComponent _attack;

        private readonly IPathMoveComponent _movement;
        private readonly ITargetRotationComponent _rotation;
        private readonly ILineOfSightComponent _lineOfSight;
        private readonly IStructureTargetSource _structureTargets;

        private Transform _player;
        private IStructureTarget _target;
        private float _nextAttackTime;
        private bool _isReloading;
        private float _reloadTimer;

        public AttackStructureState(EnemyConfig config,
            Transform transform,
            PlayerProvider playerProvider,
            Blackboard blackboard,
            IRangedAttackComponent attack,
            IPathMoveComponent movement,
            ITargetRotationComponent rotation,
            ILineOfSightComponent lineOfSight,
            [InjectOptional] IStructureTargetSource structureTargets)
        {
            _config = config;
            _transform = transform;
            _playerProvider = playerProvider;
            _blackboard = blackboard;
            _attack = attack;
            _movement = movement;
            _rotation = rotation;
            _lineOfSight = lineOfSight;
            _structureTargets = structureTargets;
        }

        /// <summary>
        /// Initializes the state and attempts to acquire a target structure.
        /// </summary>
        public void OnEnter()
        {
            _player ??= _playerProvider.Get<Transform>();

            TryAcquireTarget();
        }

        /// <summary>
        /// Approaches and fires at the target structure.
        /// </summary>
        public void OnUpdate(float deltaTime)
        {
            if (!TryAcquireTarget())
            {
                _movement.Stop();

                return;
            }

            var bounds = _target.Bounds;
            var aimPoint = GetAimPoint(bounds);

            if (bounds.SqrDistance(_transform.position) > _config.StructureAttackRange * _config.StructureAttackRange)
            {
                _movement.MoveTo(bounds.center);

                return;
            }

            _movement.Stop();
            _rotation.RotateTowards(aimPoint, _config.CombatTurnSpeed);

            if (_attack.IsEmpty)
            {
                UpdateReload(deltaTime);

                return;
            }

            if (Time.time < _nextAttackTime)
                return;

            if (!_attack.TryAttack(aimPoint, _config.StructureDamage, _config.BulletSpeed))
                return;

            _target.TakeDamage(_config.StructureDamage);

            var attackRate = Mathf.Max(_config.StructureAttackRate, 0.01f);
            _nextAttackTime = Time.time + (1f / attackRate);
        }

        /// <summary>
        /// Stops movement when exiting the state.
        /// </summary>
        public void OnExit()
        {
            _isReloading = false;
            _movement.Stop();
        }

        /// <summary>
        /// Returns true if a valid structure target exists and can be attacked.
        /// </summary>
        public bool CanAttackStructure() => TryAcquireTarget();

        /// <summary>
        /// Checks if the player is visible and reports it through the blackboard.
        /// </summary>
        public bool TrySpotPlayer()
        {
            _player ??= _playerProvider.Get<Transform>();

            if (_player == null)
                return false;

            var canSeePlayer = _lineOfSight.CheckLineOfSight(_player,
                _config.SightDistance,
                _config.ViewAngle,
                _config.ViewMask,
                out var targetPosition);

            if (!canSeePlayer)
                return false;

            _blackboard.SetValue(BlackboardTag.TargetPosition, targetPosition);

            return true;
        }

        private bool TryAcquireTarget()
        {
            if (_structureTargets == null)
                return false;

            if (_target != null && _target.IsValid)
                return true;

            return _structureTargets.TryGetNearest(_transform.position, out _target);
        }

        private void UpdateReload(float deltaTime)
        {
            if (!_isReloading)
            {
                _isReloading = true;
                _reloadTimer = _config.ReloadTime;
            }

            _reloadTimer -= deltaTime;

            if (_reloadTimer > 0f)
                return;

            _attack.Reload();
            _isReloading = false;
        }

        private Vector3 GetAimPoint(Bounds bounds)
        {
            var closestPoint = bounds.ClosestPoint(_transform.position);

            return new Vector3(closestPoint.x, bounds.center.y, closestPoint.z);
        }
    }
}
