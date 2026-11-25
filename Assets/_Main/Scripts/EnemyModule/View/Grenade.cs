using BaseModule;
using UnityEngine;
using UnityEngine.Audio;

namespace EnemyModule
{
    /// <summary>
    /// A thrown grenade: flies along an arc to the landing point and explodes when its fuse runs out.
    /// </summary>
    public sealed class Grenade : MonoBehaviour
    {
        private const float MinFlightTime = 0.3f;
        private const float MinFlightSpeed = 0.1f;
        private const float MarkerHeight = 0.02f;
        private const float MarkerLift = 0.05f;
        private const float LandSoundMaxDistance = 35f;

        [Header("References")]
        [SerializeField] private Rigidbody body;
        [SerializeField] private Collider bodyCollider;
        [SerializeField] private ExplosionFeedback explosionFeedback;
        [SerializeField] private Transform dangerMarker;

        [Header("Audio")]
        [SerializeField] private SoundCue landSound;
        [SerializeField] private AudioMixerGroup mixerGroup;

        private GrenadierConfig _config;
        private IExplosionDamageService _explosions;

        private float _remainingFuse;
        private bool _isLaunched;
        private bool _hasLanded;
        private bool _isPaused;
        private Vector3 _pausedVelocity;
        private Vector3 _pausedAngularVelocity;

        private void OnEnable()
        {
            PauseState.PauseChanged += SetPaused;
            SetPaused(PauseState.IsPaused);
        }

        private void Update()
        {
            if (!_isLaunched || _isPaused)
                return;

            _remainingFuse -= Time.deltaTime;

            if (_remainingFuse <= 0f)
                Explode();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!_isLaunched || _hasLanded || _isPaused)
                return;

            _hasLanded = true;
            landSound.PlayAt(transform.position, mixerGroup, LandSoundMaxDistance);
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }

        private void OnDisable() => PauseState.PauseChanged -= SetPaused;

        private void OnDestroy()
        {
            if (dangerMarker != null)
                Destroy(dangerMarker.gameObject);
        }

        /// <summary>
        /// Throws the grenade so that it lands at <paramref name="landingPoint"/> and starts the fuse.
        /// </summary>
        public void Launch(Vector3 landingPoint,
            GrenadierConfig config,
            IExplosionDamageService explosions,
            GameObject owner)
        {
            _config = config;
            _explosions = explosions;
            _remainingFuse = config.FuseTime;

            IgnoreOwnerCollisions(owner);
            ShowDangerMarker(landingPoint);

            body.linearVelocity = CalculateLaunchVelocity(transform.position, landingPoint, config.GrenadeFlightSpeed);
            body.angularVelocity = Random.insideUnitSphere * 5f;

            _isLaunched = true;
            SetPaused(PauseState.IsPaused);
        }

        /// <summary>
        /// Freezes or resumes the grenade together with the game.
        /// </summary>
        public void SetPaused(bool isPaused)
        {
            if (_isPaused == isPaused)
                return;

            _isPaused = isPaused;

            if (isPaused)
            {
                _pausedVelocity = body.linearVelocity;
                _pausedAngularVelocity = body.angularVelocity;
                body.isKinematic = true;
                return;
            }

            body.isKinematic = false;
            body.linearVelocity = _pausedVelocity;
            body.angularVelocity = _pausedAngularVelocity;
        }

        private static Vector3 CalculateLaunchVelocity(Vector3 start, Vector3 target, float speed)
        {
            var offset = target - start;
            var horizontalOffset = new Vector3(offset.x, 0f, offset.z);
            var flightTime = Mathf.Max(horizontalOffset.magnitude / Mathf.Max(speed, MinFlightSpeed), MinFlightTime);
            var gravity = -Physics.gravity.y;
            var verticalSpeed = (offset.y + 0.5f * gravity * flightTime * flightTime) / flightTime;

            return horizontalOffset / flightTime + Vector3.up * verticalSpeed;
        }

        private void IgnoreOwnerCollisions(GameObject owner)
        {
            if (owner == null || bodyCollider == null)
                return;

            foreach (var ownerCollider in owner.GetComponentsInChildren<Collider>())
                Physics.IgnoreCollision(bodyCollider, ownerCollider);
        }

        private void ShowDangerMarker(Vector3 landingPoint)
        {
            if (dangerMarker == null)
                return;

            if (!_config.ShouldShowDangerMarker)
            {
                dangerMarker.gameObject.SetActive(false);
                return;
            }

            var diameter = _config.ExplosionRadius * 2f;

            dangerMarker.SetParent(null, true);
            dangerMarker.SetPositionAndRotation(landingPoint + Vector3.up * MarkerLift, Quaternion.identity);
            dangerMarker.localScale = new Vector3(diameter, MarkerHeight, diameter);
            dangerMarker.gameObject.SetActive(true);
        }

        private void Explode()
        {
            _isLaunched = false;

            _explosions.ApplyToPlayer(transform.position, _config.ExplosionRadius, _config.ExplosionDamage);

            if (explosionFeedback != null)
                explosionFeedback.Play(transform.position, _config.ExplosionRadius);

            Destroy(gameObject);
        }
    }
}
