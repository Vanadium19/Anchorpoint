using ComponentsModule;
using FSMModule;
using PlayerModule;
using UnityEngine;
using UnityEngine.AI;

namespace EnemyModule
{
    /// <summary>
    /// Stands still and lobs grenades at the player, stepping back when the player comes too close.
    /// </summary>
    public sealed class ThrowState : IState
    {
        private const float RetreatOvershoot = 1f;
        private const float RetreatSampleRadius = 2f;
        private const float MinDirectionSqrMagnitude = 0.0001f;

        private readonly EnemyConfig _config;
        private readonly GrenadierConfig _grenadierConfig;
        private readonly Transform _transform;

        private readonly PlayerProvider _playerProvider;
        private readonly Blackboard _blackboard;

        private readonly IGrenadeThrowComponent _thrower;
        private readonly IPathMoveComponent _movement;
        private readonly ITargetRotationComponent _rotation;
        private readonly ILineOfSightComponent _lineOfSight;

        private Transform _player;
        private float _nextThrowTime;
        private float _windupTimer;
        private bool _isThrowing;

        public ThrowState(EnemyConfig config,
            GrenadierConfig grenadierConfig,
            Transform transform,
            PlayerProvider playerProvider,
            Blackboard blackboard,
            IGrenadeThrowComponent thrower,
            IPathMoveComponent movement,
            ITargetRotationComponent rotation,
            ILineOfSightComponent lineOfSight)
        {
            _config = config;
            _grenadierConfig = grenadierConfig;
            _transform = transform;
            _playerProvider = playerProvider;
            _blackboard = blackboard;
            _thrower = thrower;
            _movement = movement;
            _rotation = rotation;
            _lineOfSight = lineOfSight;
        }

        public void OnEnter()
        {
            _player ??= _playerProvider.Get<Transform>();
            _isThrowing = false;
            _movement.Stop();
        }

        public void OnUpdate(float deltaTime)
        {
            if (!TryUpdateVisibleTarget(out var targetPosition))
            {
                if (!_isThrowing || !_blackboard.TryGetValue(BlackboardTag.TargetPosition, out targetPosition))
                    return;
            }

            if (_isThrowing)
            {
                UpdateWindup(deltaTime, targetPosition);
                return;
            }

            if (TryRetreat(targetPosition))
                return;

            _movement.Stop();
            _rotation.RotateTowards(targetPosition, _config.CombatTurnSpeed);

            if (Time.time >= _nextThrowTime)
                BeginThrow();
        }

        public void OnExit()
        {
            _isThrowing = false;
            _movement.Stop();
        }

        public bool CanEnterThrow()
        {
            if (!TryUpdateVisibleTarget(out var targetPosition))
                return false;

            return Vector3.Distance(_transform.position, targetPosition) <= _grenadierConfig.ThrowDistance;
        }

        public bool ShouldExitThrow()
        {
            if (_isThrowing)
                return false;

            if (!TryUpdateVisibleTarget(out var targetPosition))
                return true;

            var maxThrowDistance = _grenadierConfig.ThrowDistance * _config.AttackExitRangeMultiplier;
            return Vector3.Distance(_transform.position, targetPosition) > maxThrowDistance;
        }

        private void BeginThrow()
        {
            _isThrowing = true;
            _windupTimer = _grenadierConfig.ThrowWindupDuration;
            _nextThrowTime = Time.time + _grenadierConfig.ThrowInterval;
            _movement.Stop();
            _thrower.BeginThrow();
        }

        private void UpdateWindup(float deltaTime, Vector3 targetPosition)
        {
            _movement.Stop();
            _rotation.RotateTowards(targetPosition, _config.CombatTurnSpeed);

            _windupTimer -= deltaTime;

            if (_windupTimer > 0f)
                return;

            _isThrowing = false;
            _thrower.Release(targetPosition);
        }

        private bool TryRetreat(Vector3 targetPosition)
        {
            var retreatDistance = _grenadierConfig.RetreatDistance;

            if (retreatDistance <= 0f)
                return false;

            var away = _transform.position - targetPosition;
            away.y = 0f;

            var distance = away.magnitude;

            if (distance >= retreatDistance)
                return false;

            away = away.sqrMagnitude > MinDirectionSqrMagnitude ? away / distance : -_transform.forward;

            var wantedPosition = _transform.position + away * (retreatDistance - distance + RetreatOvershoot);

            if (!NavMesh.SamplePosition(wantedPosition, out var hit, RetreatSampleRadius, NavMesh.AllAreas))
                return false;

            _movement.MoveTo(hit.position);
            return true;
        }

        private bool TryUpdateVisibleTarget(out Vector3 targetPosition)
        {
            _player ??= _playerProvider.Get<Transform>();
            targetPosition = default;

            if (_player == null)
                return false;

            var canSeePlayer = _lineOfSight.CheckLineOfSight(_player,
                _config.SightDistance,
                _config.ViewAngle,
                _config.ViewMask,
                out targetPosition);

            if (!canSeePlayer)
                return false;

            _blackboard.SetValue(BlackboardTag.TargetPosition, targetPosition);
            return true;
        }
    }
}
