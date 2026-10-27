using AudioModule;
using BaseModule;
using ComponentsModule;
using UnityEngine;

namespace WeaponModule
{
    public class Bullet : MonoBehaviour, IProjectile, IPausable
    {
        [Header("References")]
        [SerializeField] private Rigidbody rigidbodyComponent;
        [SerializeField] private TrailRenderer trailRenderer;
        [SerializeField] private Collider colliderComponent;

        [Header("Lifetime")]
        [SerializeField] private float lifeTime = 3f;

        [Header("Hit Feedback")]
        [SerializeField] private GameObject hitEffect;
        [SerializeField] private float hitEffectLifetime = 2f;
        [SerializeField, Min(0.01f)] private float hitEffectScale = 1f;
        [SerializeField] private string hitAudioEvent;
        [SerializeField] private float hitAudioMinInterval = 0.05f;

        private float _damage;
        private GameObject _owner;
        private float _remainingLifeTime;
        private Vector3 _pausedVelocity;
        private Vector3 _pausedAngularVelocity;
        private bool _isPaused;
        private bool _hasSetup;
        private bool _isHitSoundEnabled = true;
        private bool _hasHit;

        private void OnValidate()
        {
            rigidbodyComponent ??= GetComponent<Rigidbody>();
            trailRenderer ??= GetComponent<TrailRenderer>();
            colliderComponent ??= GetComponent<Collider>();
        }

        private void OnEnable()
        {
            PauseState.PauseChanged += SetPaused;
            SetPaused(PauseState.IsPaused);
        }

        private void Update()
        {
            if (_isPaused || !_hasSetup)
                return;

            _remainingLifeTime -= Time.deltaTime;

            if (_remainingLifeTime <= 0f)
                Destroy(gameObject);
        }

        private void OnDisable()
        {
            PauseState.PauseChanged -= SetPaused;
        }

        public void Setup(float damage, float bulletSpeed, float inheritFactor, Vector3 shooterVelocity, GameObject owner)
        {
            _damage = damage;
            _owner = owner;
            _remainingLifeTime = lifeTime;
            _hasSetup = true;

            IgnoreOwnerCollisions();

            if (rigidbodyComponent != null)
            {
                rigidbodyComponent.linearVelocity = Vector3.zero;
                rigidbodyComponent.angularVelocity = Vector3.zero;

                var bulletVelocity = transform.forward * bulletSpeed;
                var inheritedVelocity = shooterVelocity * inheritFactor;
                rigidbodyComponent.linearVelocity = bulletVelocity + inheritedVelocity;
            }

            if (trailRenderer != null)
            {
                trailRenderer.Clear();
                trailRenderer.enabled = true;
            }

            SetPaused(PauseState.IsPaused);
        }

        public void SetHitSoundEnabled(bool isEnabled) => _isHitSoundEnabled = isEnabled;

        public void IgnoreCollisionWith(Bullet other)
        {
            if (other == null || colliderComponent == null || other.colliderComponent == null)
                return;

            Physics.IgnoreCollision(colliderComponent, other.colliderComponent);
        }

        public void SetPaused(bool isPaused)
        {
            if (_isPaused == isPaused)
                return;

            _isPaused = isPaused;

            if (rigidbodyComponent != null)
            {
                if (isPaused)
                {
                    _pausedVelocity = rigidbodyComponent.linearVelocity;
                    _pausedAngularVelocity = rigidbodyComponent.angularVelocity;
                    rigidbodyComponent.linearVelocity = Vector3.zero;
                    rigidbodyComponent.angularVelocity = Vector3.zero;
                    rigidbodyComponent.isKinematic = true;
                }
                else
                {
                    rigidbodyComponent.isKinematic = false;
                    rigidbodyComponent.linearVelocity = _pausedVelocity;
                    rigidbodyComponent.angularVelocity = _pausedAngularVelocity;
                }
            }

            if (trailRenderer != null)
                trailRenderer.emitting = !isPaused;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_isPaused || _hasHit)
                return;

            _hasHit = true;

            if (_owner != null && collision.transform.IsChildOf(_owner.transform))
            {
                Destroy(gameObject);
                return;
            }

            if (collision.contactCount == 0)
            {
                Destroy(gameObject);
                return;
            }

            var contact = collision.GetContact(0);
            var entity = collision.gameObject.GetComponentInParent<IEntity>();

            if (entity != null && entity.TryGet<IDamageable>(out var damageable))
            {
                var force = transform.forward * 10f;
                damageable.TakeDamage(_damage, contact.point, force);
            }

            PlayHitEffect(contact);

            if (_isHitSoundEnabled)
                PlayHitSound(contact.point);

            Destroy(gameObject);
        }

        private void IgnoreOwnerCollisions()
        {
            if (_owner == null || colliderComponent == null)
                return;

            foreach (var ownerCollider in _owner.GetComponentsInChildren<Collider>())
                Physics.IgnoreCollision(colliderComponent, ownerCollider);
        }

        private void PlayHitEffect(ContactPoint contact)
        {
            if (hitEffect == null)
                return;

            var position = contact.point + contact.normal * 0.05f;
            var rotation = Quaternion.LookRotation(contact.normal);
            var effect = Instantiate(hitEffect, position, rotation);
            effect.transform.localScale *= hitEffectScale;
            Destroy(effect, hitEffectLifetime);
        }

        private void PlayHitSound(Vector3 position)
        {
            if (string.IsNullOrEmpty(hitAudioEvent))
                return;

            AudioSystem.Resolve(this).PlayEvent(hitAudioEvent, position, hitAudioMinInterval);
        }
    }
}
