using BaseModule;
using ComponentsModule;
using UnityEngine;
using Zenject;

namespace EnemyModule
{
    /// <summary>
    /// Plays the drone sounds: the rotor hum that follows the speed, the alert chirp, the dodge burst and the hit clang.
    /// </summary>
    public sealed class DroneAudioView : MonoBehaviour, IPausable
    {
        [Header("References")]
        [SerializeField] private AudioSource humSource;
        [SerializeField] private AudioSource effectSource;

        [Header("Hum")]
        [SerializeField] private float idleHumPitch = 0.85f;
        [SerializeField] private float cruiseHumPitch = 1.25f;
        [SerializeField] private float fuseHumPitch = 1.6f;
        [SerializeField] private float humPitchSharpness = 5f;

        [Header("Sounds")]
        [SerializeField] private SoundCue alertSound;
        [SerializeField] private SoundCue dodgeSound;
        [SerializeField] private SoundCue hitSound;

        private DroneConfig _config;
        private IDroneExplosive _explosive;
        private IDroneFlightComponent _flight;
        private IHealthComponent _health;
        private IPauseManager _pauseManager;

        private bool _wasChasing;
        private bool _wasDodging;
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
            _health.DamageTaken += OnDamageTaken;
        }

        private void Start()
        {
            humSource.pitch = idleHumPitch;
            humSource.loop = true;
            humSource.Play();
        }

        private void Update()
        {
            if (_isPaused)
                return;

            UpdateEvents();
            UpdateHumPitch(Time.deltaTime);
        }

        private void OnDisable()
        {
            _explosive.FuseLit -= OnFuseLit;
            _health.DamageTaken -= OnDamageTaken;
        }

        private void OnDestroy() => _pauseManager?.Unregister(this);

        public void SetPaused(bool isPaused)
        {
            _isPaused = isPaused;

            if (isPaused)
                humSource.Pause();
            else
                humSource.UnPause();
        }

        private void UpdateEvents()
        {
            var isChasing = _flight.HasDestination;
            var isDodging = _flight.IsDodging;

            if (isChasing && !_wasChasing)
                alertSound.Play(effectSource);

            if (isDodging && !_wasDodging)
                dodgeSound.Play(effectSource);

            _wasChasing = isChasing;
            _wasDodging = isDodging;
        }

        private void UpdateHumPitch(float deltaTime)
        {
            var speedFactor = Mathf.Clamp01(_flight.Velocity.magnitude / Mathf.Max(_config.MoveSpeed, 0.1f));
            var targetPitch = _isFuseLit ? fuseHumPitch : Mathf.Lerp(idleHumPitch, cruiseHumPitch, speedFactor);
            var blend = 1f - Mathf.Exp(-humPitchSharpness * deltaTime);

            humSource.pitch = Mathf.Lerp(humSource.pitch, targetPitch, blend);
        }

        private void OnFuseLit() => _isFuseLit = true;

        private void OnDamageTaken(Vector3? hitPosition, Vector3? hitForce) => hitSound.Play(effectSource);
    }
}
