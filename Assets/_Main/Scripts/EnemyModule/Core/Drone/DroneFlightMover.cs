using System.Collections.Generic;
using BaseModule;
using ComponentsModule;
using UnityEngine;
using Zenject;

namespace EnemyModule
{
    /// <summary>
    /// Moves the drone through the air without a NavMesh: smoothly accelerates toward the destination with simple obstacle avoidance.
    /// </summary>
    public sealed class DroneFlightMover : MonoBehaviour, IDroneFlightComponent, IPausable
    {
        private const float ArrivalDistance = 0.05f;
        private const float MinMovingSqrSpeed = 0.01f;
        private const float MinDirectionSqrMagnitude = 0.0001f;
        private const float MinSpeedMultiplier = 0.01f;
        private const float GroundProbeHeight = 1f;
        private const float GroundProbeDistance = 100f;
        private const float AvoidanceLift = 0.3f;
        private const float FullCircleRadians = Mathf.PI * 2f;
        private const float HoverSmoothTime = 0.4f;
        private const float IdleTurnSpeed = 18f;
        private const float MinFacingSpeed = 0.6f;

        private readonly Dictionary<object, float> _speedMultipliers = new();

        private DroneConfig _config;
        private IPauseManager _pauseManager;

        private Vector3 _destination;
        private Vector3 _velocity;
        private Vector3 _dodgeStart;
        private Vector3 _dodgeOffset;
        private float _dodgeDuration;
        private float _dodgeElapsed;
        private float _hoverPhase;
        private float _verticalVelocity;
        private float _stoppingDistance;
        private bool _hasDestination;
        private bool _isDodging;
        private bool _isPaused;

        [Inject]
        public void Construct(DroneConfig config, IPauseManager pauseManager)
        {
            _config = config;
            _pauseManager = pauseManager;
            _pauseManager.Register(this);
        }

        public bool IsMoving => _velocity.sqrMagnitude > MinMovingSqrSpeed;
        public bool IsPathPending => false;
        public float RemainingDistance => _hasDestination ? Vector3.Distance(transform.position, _destination) : 0f;
        public float StoppingDistance => _stoppingDistance;
        public Vector3 Velocity => _velocity;
        public float BaseSpeed => _config != null ? _config.MoveSpeed : 0f;
        public float CurrentSpeed => BaseSpeed * GetSpeedMultiplier();
        public bool IsDodging => _isDodging;
        public bool HasDestination => _hasDestination;

        private void Start()
        {
            _hoverPhase = Random.value * FullCircleRadians;
            SnapToHoverHeight();
        }

        private void Update()
        {
            if (_isPaused)
                return;

            Tick(Time.deltaTime);
        }

        private void OnDestroy() => _pauseManager?.Unregister(this);

        public void SetPaused(bool isPaused) => _isPaused = isPaused;

        public void SetStoppingDistance(float value) => _stoppingDistance = value;

        public void MoveTo(Vector3 position)
        {
            _destination = position;
            _hasDestination = true;
        }

        public void Stop() => _hasDestination = false;

        public void ResetPath() => _hasDestination = false;

        public void ApplySpeedMultiplier(object source, float multiplier)
        {
            if (source == null)
                return;

            _speedMultipliers[source] = Mathf.Clamp(multiplier, MinSpeedMultiplier, 1f);
        }

        public void RemoveSpeedMultiplier(object source)
        {
            if (source == null)
                return;

            _speedMultipliers.Remove(source);
        }

        public void Dodge(Vector3 direction, float distance, float duration)
        {
            if (direction.sqrMagnitude < MinDirectionSqrMagnitude || duration <= 0f)
                return;

            _dodgeStart = transform.position;
            _dodgeOffset = direction.normalized * distance;
            _dodgeDuration = duration;
            _dodgeElapsed = 0f;
            _isDodging = true;
        }

        private void Tick(float deltaTime)
        {
            if (_isDodging)
                TickDodge(deltaTime);
            else if (_hasDestination)
                TickFlight(deltaTime);
            else
                TickHover(deltaTime);
        }

        private void TickDodge(float deltaTime)
        {
            var previousPosition = transform.position;

            _dodgeElapsed += deltaTime;

            var progress = Mathf.Clamp01(_dodgeElapsed / _dodgeDuration);
            var easedProgress = 1f - Mathf.Pow(1f - progress, 3f);

            transform.position = _dodgeStart + _dodgeOffset * easedProgress;
            _velocity = (transform.position - previousPosition) / deltaTime;

            if (progress >= 1f)
                _isDodging = false;
        }

        private void TickFlight(float deltaTime)
        {
            var toDestination = _destination - transform.position;
            var distance = toDestination.magnitude;

            if (distance <= Mathf.Max(ArrivalDistance, _stoppingDistance))
            {
                _velocity = Vector3.MoveTowards(_velocity, Vector3.zero, _config.Deceleration * deltaTime);
                return;
            }

            var desiredVelocity = AvoidObstacles(toDestination / distance) * CurrentSpeed;
            var rate = desiredVelocity.sqrMagnitude >= _velocity.sqrMagnitude ? _config.Acceleration : _config.Deceleration;

            _velocity = Vector3.MoveTowards(_velocity, desiredVelocity, rate * deltaTime);
            transform.position += Vector3.ClampMagnitude(_velocity * deltaTime, distance);
            FaceVelocity(deltaTime);
        }

        private void TickHover(float deltaTime)
        {
            _hoverPhase += deltaTime * FullCircleRadians / _config.HoverPeriod;

            var horizontalVelocity = new Vector3(_velocity.x, 0f, _velocity.z);
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, _config.Deceleration * deltaTime);

            var position = transform.position + horizontalVelocity * deltaTime;

            if (TryGetGroundHeight(position, out var groundHeight))
            {
                var hoverHeight = groundHeight + _config.FlightHeight + Mathf.Sin(_hoverPhase) * _config.HoverAmplitude;
                position.y = Mathf.SmoothDamp(position.y, hoverHeight, ref _verticalVelocity, HoverSmoothTime);
            }
            else
            {
                _verticalVelocity = 0f;
            }

            _velocity = new Vector3(horizontalVelocity.x, _verticalVelocity, horizontalVelocity.z);
            transform.position = position;
            transform.Rotate(0f, IdleTurnSpeed * deltaTime, 0f, Space.World);
        }

        private void SnapToHoverHeight()
        {
            var position = transform.position;

            if (TryGetGroundHeight(position, out var groundHeight))
                transform.position = new Vector3(position.x, groundHeight + _config.FlightHeight, position.z);
        }

        private Vector3 AvoidObstacles(Vector3 direction)
        {
            var isBlocked = Physics.SphereCast(transform.position,
                _config.ObstacleCheckRadius,
                direction,
                out var hit,
                _config.ObstacleCheckDistance,
                _config.ObstacleMask,
                QueryTriggerInteraction.Ignore);

            if (!isBlocked || IsOwnCollider(hit.collider) || hit.collider.GetComponentInParent<IEntity>() != null)
                return direction;

            var slide = Vector3.ProjectOnPlane(direction, hit.normal);

            if (slide.sqrMagnitude < MinDirectionSqrMagnitude)
                slide = Vector3.up;

            var steered = slide.normalized + hit.normal * _config.AvoidanceStrength + Vector3.up * AvoidanceLift;
            return steered.normalized;
        }

        private bool TryGetGroundHeight(Vector3 position, out float groundHeight)
        {
            var origin = position + Vector3.up * GroundProbeHeight;
            var hasGround = Physics.Raycast(origin,
                Vector3.down,
                out var hit,
                GroundProbeDistance,
                _config.ObstacleMask,
                QueryTriggerInteraction.Ignore);

            groundHeight = hasGround ? hit.point.y : position.y;
            return hasGround;
        }

        private void FaceVelocity(float deltaTime)
        {
            var direction = new Vector3(_velocity.x, 0f, _velocity.z);

            if (direction.sqrMagnitude < MinFacingSpeed * MinFacingSpeed)
                return;

            var targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _config.TurnSpeed * deltaTime);
        }

        private bool IsOwnCollider(Collider other) => other.transform.IsChildOf(transform);

        private float GetSpeedMultiplier()
        {
            var multiplier = 1f;

            foreach (var value in _speedMultipliers.Values)
                multiplier = Mathf.Min(multiplier, value);

            return multiplier;
        }
    }
}
