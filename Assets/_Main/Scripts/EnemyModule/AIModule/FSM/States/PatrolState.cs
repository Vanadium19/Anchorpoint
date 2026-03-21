using System;
using ComponentsModule;
using FSMModule;
using PlayerModule;
using UnityEngine;

namespace EnemyModule
{
    public sealed class PatrolState : IState
    {
        private const float PatrolLookDuration = 1f;

        private readonly EnemyConfig _config;
        private readonly Transform _transform;
        private readonly Transform[] _patrolPoints;

        private readonly PlayerProvider _playerProvider;
        private readonly Blackboard _blackboard;

        private readonly IHealthComponent _health;
        private readonly IPathMoveComponent _movement;
        private readonly ITargetRotationComponent _rotation;
        private readonly ILineOfSightComponent _lineOfSight;

        private Transform _player;

        private bool _isAlertedByDamage;

        private bool _isWaiting;

        private float _waitTimer;
        private float _nextLookTimer;

        private int _currentPointIndex;

        private bool _hasLookTarget;
        private float _lookTurnSpeed;
        private Quaternion _lookTargetRotation;

        public PatrolState(EnemyConfig config,
            Transform transform,
            PlayerProvider playerProvider,
            IHealthComponent health,
            Blackboard blackboard,
            IPathMoveComponent movement,
            ITargetRotationComponent rotation,
            ILineOfSightComponent lineOfSight,
            Transform[] patrolPoints)
        {
            _config = config;
            _transform = transform;
            _playerProvider = playerProvider;
            _health = health;
            _blackboard = blackboard;
            _movement = movement;
            _rotation = rotation;
            _lineOfSight = lineOfSight;
            _patrolPoints = patrolPoints ?? Array.Empty<Transform>();
        }

        public void OnEnter()
        {
            _player ??= _playerProvider.Get<Transform>();
            _blackboard.DeleteValue<Vector3>(BlackboardTag.TargetPosition);

            _health.DamageTaken += OnDamageTaken;

            _isAlertedByDamage = false;
            _isWaiting = false;
            _hasLookTarget = false;

            _waitTimer = 0f;
            _nextLookTimer = 0f;
            _lookTurnSpeed = 0f;

            MoveToCurrentPoint();
        }

        public void OnUpdate(float deltaTime)
        {
            if (_patrolPoints.Length == 0)
                return;

            if (_isWaiting)
            {
                UpdateWaiting(deltaTime);
                return;
            }

            if (_movement.IsPathPending || _movement.RemainingDistance > _config.StoppingDistance)
                return;

            BeginWaiting();
        }

        public void OnExit()
        {
            _health.DamageTaken -= OnDamageTaken;

            _isAlertedByDamage = false;
            _isWaiting = false;
            _hasLookTarget = false;
        }

        private void BeginWaiting()
        {
            _isWaiting = true;
            _waitTimer = _config.PatrolWaitTime;
            _nextLookTimer = 0f;
            _hasLookTarget = false;
            _movement.Stop();
        }

        private void UpdateWaiting(float deltaTime)
        {
            _waitTimer -= deltaTime;
            _nextLookTimer -= deltaTime;

            if (_nextLookTimer <= 0f)
                PickNextLookTarget();

            if (_hasLookTarget)
                UpdateLookRotation();

            if (_waitTimer > 0f)
                return;

            _isWaiting = false;
            _currentPointIndex = (_currentPointIndex + 1) % _patrolPoints.Length;
            MoveToCurrentPoint();
        }

        private void PickNextLookTarget()
        {
            _nextLookTimer = Mathf.Max(_config.LookInterval, 0.1f);

            var angleOffset = UnityEngine.Random.Range(-_config.LookAngleRange, _config.LookAngleRange);
            _lookTargetRotation = _transform.rotation * Quaternion.Euler(0f, angleOffset, 0f);
            _lookTurnSpeed = Quaternion.Angle(_transform.rotation, _lookTargetRotation) / PatrolLookDuration;
            _hasLookTarget = true;
        }

        private void UpdateLookRotation()
        {
            _rotation.RotateTo(_lookTargetRotation, _lookTurnSpeed);

            if (Quaternion.Angle(_transform.rotation, _lookTargetRotation) <= 0.1f)
                _hasLookTarget = false;
        }

        public bool TrySpotPlayer()
        {
            if (_isAlertedByDamage && _blackboard.HasValue<Vector3>(BlackboardTag.TargetPosition))
                return true;

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

        private void MoveToCurrentPoint()
        {
            if (_patrolPoints.Length == 0)
            {
                _movement.Stop();
                return;
            }

            _movement.MoveTo(_patrolPoints[_currentPointIndex].position);
        }

        private void OnDamageTaken(Vector3? hitPosition, Vector3? hitForce)
        {
            _player ??= _playerProvider.Get<Transform>();

            if (_player == null)
                return;

            _blackboard.SetValue(BlackboardTag.TargetPosition, _player.position);
            _isAlertedByDamage = true;
        }
    }
}