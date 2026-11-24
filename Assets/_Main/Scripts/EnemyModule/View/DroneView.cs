using BaseModule;
using ComponentsModule;
using UnityEngine;
using Zenject;

namespace EnemyModule
{
    /// <summary>
    /// Shows the drone: banking into turns, spinning rotors, the status light, the hit shake, the dodge trail and the fuse countdown.
    /// </summary>
    public sealed class DroneView : MonoBehaviour, IPausable
    {
        private const float FullCircleRadians = Mathf.PI * 2f;
        private const float AlertDuration = 0.4f;
        private const float AlertLift = 0.35f;
        private const float AlertPitch = -14f;
        private const float FuseMinBlinkInterval = 0.035f;
        private const float FuseMaxBeepPitch = 1.7f;
        private const float FusePulseScale = 0.1f;
        private const float WobbleAngle = 2.5f;
        private const float WobbleFrequency = 0.9f;
        private const float StatusEmission = 2f;
        private const float StatusLightIntensity = 1.6f;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        [Header("References")]
        [SerializeField] private Transform visual;
        [SerializeField] private Transform[] rotors;
        [SerializeField] private Renderer statusRenderer;
        [SerializeField] private Light statusLight;
        [SerializeField] private TrailRenderer dodgeTrail;
        [SerializeField] private AudioSource beepSource;
        [SerializeField] private SoundCue beepSound;
        [SerializeField] private ExplosionFeedback explosionFeedback;

        [Header("Banking")]
        [SerializeField] private float cruiseBankAngle = 22f;
        [SerializeField] private float dodgeBankAngle = 60f;
        [SerializeField] private float maxPitchAngle = 24f;
        [SerializeField] private float tiltSharpness = 7f;

        [Header("Hover")]
        [SerializeField] private float bobAmplitude = 0.07f;
        [SerializeField] private float bobFrequency = 1.6f;

        [Header("Rotors")]
        [SerializeField] private float idleSpinSpeed = 1200f;
        [SerializeField] private float cruiseSpinSpeed = 2400f;
        [SerializeField] private float fuseSpinSpeed = 3400f;
        [SerializeField] private float spinChangeRate = 2500f;

        [Header("Hit Shake")]
        [SerializeField] private float shakeDuration = 0.18f;
        [SerializeField] private float shakeAmount = 0.14f;

        [Header("Status Colors")]
        [SerializeField] private Color idleColor = new(0.2f, 1f, 0.35f);
        [SerializeField] private Color chaseColor = new(1f, 0.5f, 0.05f);
        [SerializeField] private Color fuseColor = new(1f, 0.08f, 0.04f);

        private MaterialPropertyBlock _block;
        private DroneConfig _config;
        private IDroneExplosive _explosive;
        private IDroneFlightComponent _flight;
        private IHealthComponent _health;
        private IPauseManager _pauseManager;

        private Vector3 _visualOrigin;
        private Vector3 _visualScale;
        private Quaternion _tilt = Quaternion.identity;

        private float _spinSpeed;
        private float _bobPhase;
        private float _alertTimer;
        private float _shakeTimer;
        private float _fuseElapsed;
        private float _blinkTimer;
        private bool _isBlinkOn;
        private bool _wasChasing;
        private bool _isFuseLit;
        private bool _isPaused;

        [Inject]
        public void Construct(DroneConfig config,
            IDroneExplosive explosive,
            IDroneFlightComponent flight,
            IHealthComponent health,
            IPauseManager pauseManager)
        {
            _config = config;
            _explosive = explosive;
            _flight = flight;
            _health = health;
            _pauseManager = pauseManager;
            _pauseManager.Register(this);
        }

        private void OnEnable()
        {
            _explosive.FuseLit += OnFuseLit;
            _explosive.Detonated += OnDetonated;
            _health.DamageTaken += OnDamageTaken;
        }

        private void Start()
        {
            _block = new MaterialPropertyBlock();
            _visualOrigin = visual.localPosition;
            _visualScale = visual.localScale;
            _spinSpeed = idleSpinSpeed;
            _bobPhase = Random.value * FullCircleRadians;

            ApplyStatus(idleColor, 1f);
            SetTrailEmitting(false);
        }

        private void LateUpdate()
        {
            if (_isPaused)
                return;

            var deltaTime = Time.deltaTime;

            UpdateChaseState();
            UpdateTilt(deltaTime);
            UpdateVisualOffset(deltaTime);
            UpdateRotors(deltaTime);
            UpdateStatus(deltaTime);
            SetTrailEmitting(_flight.IsDodging);
        }

        private void OnDisable()
        {
            _explosive.FuseLit -= OnFuseLit;
            _explosive.Detonated -= OnDetonated;
            _health.DamageTaken -= OnDamageTaken;
        }

        private void OnDestroy() => _pauseManager?.Unregister(this);

        public void SetPaused(bool isPaused) => _isPaused = isPaused;

        private void UpdateChaseState()
        {
            var isChasing = _flight.HasDestination;

            if (isChasing && !_wasChasing)
                _alertTimer = AlertDuration;

            _wasChasing = isChasing;
        }

        private void UpdateTilt(float deltaTime)
        {
            var referenceSpeed = Mathf.Max(_config.MoveSpeed, 0.1f);
            var localVelocity = transform.InverseTransformDirection(_flight.Velocity);
            var sideFactor = Mathf.Clamp(localVelocity.x / referenceSpeed, -1f, 1f);
            var forwardFactor = Mathf.Clamp(localVelocity.z / referenceSpeed, -1f, 1f);

            var bankAngle = _flight.IsDodging ? dodgeBankAngle : cruiseBankAngle;
            var pitch = _flight.IsDodging ? 0f : forwardFactor * maxPitchAngle;
            var roll = -sideFactor * bankAngle;

            if (!_wasChasing && !_flight.IsDodging)
                roll += Mathf.Sin(Time.time * WobbleFrequency * FullCircleRadians) * WobbleAngle;

            if (_alertTimer > 0f)
                pitch += AlertPitch * Mathf.Sin(Mathf.PI * (1f - _alertTimer / AlertDuration));

            var target = Quaternion.Euler(pitch, 0f, roll);
            var blend = 1f - Mathf.Exp(-tiltSharpness * (_flight.IsDodging ? 2f : 1f) * deltaTime);

            _tilt = Quaternion.Slerp(_tilt, target, blend);
            visual.localRotation = _tilt;
        }

        private void UpdateVisualOffset(float deltaTime)
        {
            _bobPhase += deltaTime * bobFrequency * FullCircleRadians;

            var offset = Vector3.up * (Mathf.Sin(_bobPhase) * bobAmplitude);

            if (_alertTimer > 0f)
            {
                offset += Vector3.up * (AlertLift * Mathf.Sin(Mathf.PI * (1f - _alertTimer / AlertDuration)));
                _alertTimer -= deltaTime;
            }

            if (_shakeTimer > 0f)
            {
                offset += Random.insideUnitSphere * (shakeAmount * (_shakeTimer / shakeDuration));
                _shakeTimer -= deltaTime;
            }

            visual.localPosition = _visualOrigin + offset;
            visual.localScale = _visualScale * (1f + (_isFuseLit && _isBlinkOn ? FusePulseScale : 0f));
        }

        private void UpdateRotors(float deltaTime)
        {
            var speedFactor = Mathf.Clamp01(_flight.Velocity.magnitude / Mathf.Max(_config.MoveSpeed, 0.1f));
            var targetSpin = Mathf.Lerp(idleSpinSpeed, cruiseSpinSpeed, speedFactor);

            if (_isFuseLit)
                targetSpin = fuseSpinSpeed;

            _spinSpeed = Mathf.MoveTowards(_spinSpeed, targetSpin, spinChangeRate * deltaTime);

            for (var i = 0; i < rotors.Length; i++)
            {
                var direction = i % 2 == 0 ? 1f : -1f;
                rotors[i].Rotate(Vector3.up, _spinSpeed * direction * deltaTime, Space.Self);
            }
        }

        private void UpdateStatus(float deltaTime)
        {
            if (_isFuseLit)
            {
                UpdateFuseCountdown(deltaTime);
                return;
            }

            ApplyStatus(_wasChasing ? chaseColor : idleColor, 1f);
        }

        private void UpdateFuseCountdown(float deltaTime)
        {
            _fuseElapsed += deltaTime;
            _blinkTimer -= deltaTime;

            if (_blinkTimer > 0f)
                return;

            var progress = Mathf.Clamp01(_fuseElapsed / Mathf.Max(_config.FuseDelay, 0.01f));

            _blinkTimer = Mathf.Lerp(_config.FuseBlinkInterval, FuseMinBlinkInterval, progress);
            _isBlinkOn = !_isBlinkOn;

            ApplyStatus(_isBlinkOn ? Color.Lerp(fuseColor, Color.white, 0.55f) : fuseColor * 0.15f, _isBlinkOn ? 2.2f : 0.2f);

            if (_isBlinkOn)
                PlayBeep(Mathf.Lerp(1f, FuseMaxBeepPitch, progress));
        }

        private void ApplyStatus(Color color, float intensity)
        {
            if (statusRenderer != null)
            {
                _block.SetColor(BaseColorId, color);
                _block.SetColor(EmissionColorId, color * (StatusEmission * intensity));
                statusRenderer.SetPropertyBlock(_block);
            }

            if (statusLight != null)
            {
                statusLight.color = color;
                statusLight.intensity = StatusLightIntensity * intensity;
            }
        }

        private void PlayBeep(float pitch) => beepSound.Play(beepSource, pitch);

        private void SetTrailEmitting(bool isEmitting)
        {
            if (dodgeTrail != null && dodgeTrail.emitting != isEmitting)
                dodgeTrail.emitting = isEmitting;
        }

        private void OnFuseLit()
        {
            _isFuseLit = true;
            _fuseElapsed = 0f;
            _blinkTimer = 0f;
        }

        private void OnDamageTaken(Vector3? hitPosition, Vector3? hitForce) => _shakeTimer = shakeDuration;

        private void OnDetonated(Vector3 position, float radius)
        {
            if (explosionFeedback != null)
                explosionFeedback.Play(position, radius);

            Destroy(gameObject);
        }
    }
}
