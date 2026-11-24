using ComponentsModule;
using UnityEngine;
using Zenject;

namespace EnemyModule
{
    /// <summary>
    /// Plays the footsteps, the grenade throw and the death sounds of a walking enemy.
    /// </summary>
    public sealed class EnemyAudioView : MonoBehaviour
    {
        [SerializeField] private AudioSource source;
        [SerializeField] [Min(0.1f)] private float stepDistance = 1.9f;

        [Header("Sounds")]
        [SerializeField] private SoundCue footstepSound;
        [SerializeField] private SoundCue grenadePrimeSound;
        [SerializeField] private SoundCue grenadeThrowSound;
        [SerializeField] private SoundCue deathSound;

        private IHealthComponent _health;
        private IPathMoveComponent _movement;
        private IGrenadeThrowComponent _grenadeThrow;

        private Vector3 _lastPosition;
        private float _walkedDistance;

        [Inject]
        public void Construct(IHealthComponent health,
            IPathMoveComponent movement,
            [InjectOptional] IGrenadeThrowComponent grenadeThrow)
        {
            _health = health;
            _movement = movement;
            _grenadeThrow = grenadeThrow;
        }

        private void OnEnable()
        {
            _health.Died += OnDied;

            if (_grenadeThrow == null)
                return;

            _grenadeThrow.ThrowStarted += OnThrowStarted;
            _grenadeThrow.ThrowReleased += OnThrowReleased;
        }

        private void Start() => _lastPosition = transform.position;

        private void Update()
        {
            var position = transform.position;
            var delta = new Vector3(position.x - _lastPosition.x, 0f, position.z - _lastPosition.z).magnitude;

            _lastPosition = position;

            if (!_movement.IsMoving)
            {
                _walkedDistance = 0f;
                return;
            }

            _walkedDistance += delta;

            if (_walkedDistance < stepDistance)
                return;

            _walkedDistance -= stepDistance;
            footstepSound.Play(source);
        }

        private void OnDisable()
        {
            _health.Died -= OnDied;

            if (_grenadeThrow == null)
                return;

            _grenadeThrow.ThrowStarted -= OnThrowStarted;
            _grenadeThrow.ThrowReleased -= OnThrowReleased;
        }

        private void OnThrowStarted() => grenadePrimeSound.Play(source);

        private void OnThrowReleased() => grenadeThrowSound.Play(source);

        private void OnDied()
        {
            enabled = false;
            deathSound.Play(source);
        }
    }
}
