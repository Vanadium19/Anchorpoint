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
        private readonly Enemy _enemy;
        private readonly EnemyView _view;
        private readonly PlayerProvider _player;
        private readonly IHealthComponent _health;
        private readonly IPathMoveComponent _movement;
        private readonly ITargetRotationComponent _rotation;
        private readonly ILineOfSightComponent _lineOfSight;
        private readonly Transform[] _patrolPoints;

        private Transform _playerTransform;
        private bool _isSubscribedToDamage;
        private bool _isAlertedByDamage;
        private bool _isWaiting;
        private bool _hasLookTarget;
        private float _waitTimer;
        private float _nextLookTimer;
        private float _lookTurnSpeed;
        private Quaternion _lookTargetRotation;
        private int _currentPointIndex;

        public PatrolState(
            EnemyConfig config,
            Enemy enemy,
            EnemyView view,
            PlayerProvider player,
            IHealthComponent health,
            IPathMoveComponent movement,
            ITargetRotationComponent rotation,
            ILineOfSightComponent lineOfSight,
            Transform[] patrolPoints)
        {
            _config = config;
            _enemy = enemy;
            _view = view;
            _player = player;
            _health = health;
            _movement = movement;
            _rotation = rotation;
            _lineOfSight = lineOfSight;
            _patrolPoints = patrolPoints ?? Array.Empty<Transform>();
        }

        public void OnEnter()
        {
            _playerTransform ??= ResolvePlayerTransform();
            SubscribeToDamage();
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
            UnsubscribeFromDamage();
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

            float angleOffset = UnityEngine.Random.Range(-_config.LookAngleRange, _config.LookAngleRange);
            _lookTargetRotation = _view.transform.rotation * Quaternion.Euler(0f, angleOffset, 0f);
            _lookTurnSpeed = Quaternion.Angle(_view.transform.rotation, _lookTargetRotation) / PatrolLookDuration;
            _hasLookTarget = true;
        }

        private void UpdateLookRotation()
        {
            _rotation.RotateTo(_lookTargetRotation, _lookTurnSpeed);

            if (Quaternion.Angle(_view.transform.rotation, _lookTargetRotation) <= 0.1f)
                _hasLookTarget = false;
        }

        public bool TrySpotPlayer()
        {
            if (_isAlertedByDamage)
                return true;

            _playerTransform ??= ResolvePlayerTransform();

            if (_playerTransform == null)
                return false;

            bool canSeePlayer = _lineOfSight.CheckLineOfSight(
                _playerTransform,
                _config.SightDistance,
                _config.ViewAngle,
                _config.ViewMask,
                out Vector3 targetPosition);

            if (!canSeePlayer)
                return false;

            _enemy.SetTargetPosition(targetPosition);
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

        private Transform ResolvePlayerTransform()
        {
            if (_player.TryGet(out Transform playerTransform))
                return playerTransform;

            return null;
        }

        private void OnDamageTaken(Vector3? hitPosition, Vector3? hitForce)
        {
            _playerTransform ??= ResolvePlayerTransform();

            if (_playerTransform == null)
                return;

            _enemy.SetTargetPosition(_playerTransform.position);
            _isAlertedByDamage = true;
        }

        private void SubscribeToDamage()
        {
            if (_isSubscribedToDamage)
                return;

            _health.DamageTaken += OnDamageTaken;
            _isSubscribedToDamage = true;
        }

        private void UnsubscribeFromDamage()
        {
            if (!_isSubscribedToDamage)
                return;

            _health.DamageTaken -= OnDamageTaken;
            _isSubscribedToDamage = false;
        }
    }
}
