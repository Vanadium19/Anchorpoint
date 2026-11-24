using ComponentsModule;
using FSMModule;
using PlayerModule;
using UnityEngine;

namespace EnemyModule
{
    /// <summary>
    /// Flies at the player at full speed, dodges incoming shots and detonates when it gets close.
    /// </summary>
    public sealed class KamikazeState : IState
    {
        private const float PlayerCenterHeight = 1f;
        private const float PercentToFraction = 0.01f;

        private readonly EnemyConfig _config;
        private readonly DroneConfig _droneConfig;
        private readonly Transform _transform;

        private readonly PlayerProvider _playerProvider;
        private readonly Blackboard _blackboard;

        private readonly IDroneFlightComponent _flight;
        private readonly IDroneExplosive _explosive;

        private Transform _player;
        private Collider _playerCollider;
        private IShotNotifier _shotNotifier;

        private float _fuseTimer;
        private float _nextDodgeTime;

        public KamikazeState(EnemyConfig config,
            DroneConfig droneConfig,
            Transform transform,
            PlayerProvider playerProvider,
            Blackboard blackboard,
            IDroneFlightComponent flight,
            IDroneExplosive explosive)
        {
            _config = config;
            _droneConfig = droneConfig;
            _transform = transform;
            _playerProvider = playerProvider;
            _blackboard = blackboard;
            _flight = flight;
            _explosive = explosive;
        }

        public void OnEnter()
        {
            ResolvePlayer();
            SubscribeToShots();
        }

        public void OnUpdate(float deltaTime)
        {
            if (!TryGetTargetPosition(out var targetPosition))
            {
                _flight.Stop();
                return;
            }

            _blackboard.SetValue(BlackboardTag.TargetPosition, targetPosition);
            _flight.MoveTo(targetPosition);

            UpdateFuse(deltaTime, targetPosition);
        }

        public void OnExit()
        {
            UnsubscribeFromShots();
            _flight.Stop();
        }

        public bool HasLostTarget()
        {
            if (!TryGetTargetPosition(out var targetPosition))
                return true;

            if (_config.LoseTargetDistance <= 0f)
                return false;

            return Vector3.Distance(_transform.position, targetPosition) > _config.LoseTargetDistance;
        }

        private void UpdateFuse(float deltaTime, Vector3 targetPosition)
        {
            if (!_explosive.IsFuseLit)
            {
                if (Vector3.Distance(_transform.position, targetPosition) > _droneConfig.FuseTriggerDistance)
                    return;

                _explosive.LightFuse();
                _fuseTimer = _droneConfig.FuseDelay;
                return;
            }

            _fuseTimer -= deltaTime;

            if (_fuseTimer <= 0f)
                _explosive.Detonate();
        }

        private void ResolvePlayer()
        {
            _player ??= _playerProvider.Get<Transform>();

            if (_player != null && _playerCollider == null)
                _player.TryGetComponent(out _playerCollider);
        }

        private bool TryGetTargetPosition(out Vector3 targetPosition)
        {
            ResolvePlayer();

            if (_player == null)
            {
                targetPosition = default;
                return false;
            }

            targetPosition = _playerCollider != null
                ? _playerCollider.bounds.center
                : _player.position + Vector3.up * PlayerCenterHeight;

            return true;
        }

        private void SubscribeToShots()
        {
            if (_shotNotifier != null || !_playerProvider.TryGet<IShotNotifier>(out _shotNotifier))
                return;

            _shotNotifier.ShotFired += OnShotFired;
        }

        private void UnsubscribeFromShots()
        {
            if (_shotNotifier == null)
                return;

            _shotNotifier.ShotFired -= OnShotFired;
            _shotNotifier = null;
        }

        private void OnShotFired(Vector3 origin, Vector3 direction)
        {
            if (_flight.IsDodging || Time.time < _nextDodgeTime)
                return;

            if (!IsAimedAt(origin, direction))
                return;

            if (Random.value >= _droneConfig.EvadeChancePercent * PercentToFraction)
                return;

            _flight.Dodge(PickDodgeDirection(direction), _droneConfig.EvadeDistance, _droneConfig.EvadeDuration);
            _nextDodgeTime = Time.time + _droneConfig.EvadeDuration + _droneConfig.EvadeCooldown;
        }

        private bool IsAimedAt(Vector3 origin, Vector3 direction)
        {
            var distanceAlongShot = Vector3.Dot(_transform.position - origin, direction);

            if (distanceAlongShot <= 0f)
                return false;

            var closestPoint = origin + direction * distanceAlongShot;
            var triggerRadius = _droneConfig.EvadeTriggerRadius;

            return (_transform.position - closestPoint).sqrMagnitude <= triggerRadius * triggerRadius;
        }

        private Vector3 PickDodgeDirection(Vector3 shotDirection)
        {
            var side = Vector3.Cross(Vector3.up, shotDirection);

            if (side.sqrMagnitude < 0.0001f)
                side = _transform.right;

            side.Normalize();

            return Random.Range(0, 3) switch
            {
                0 => side,
                1 => -side,
                _ => Vector3.up,
            };
        }
    }
}
