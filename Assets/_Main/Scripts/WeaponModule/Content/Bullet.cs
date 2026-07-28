using BaseModule;
using ComponentsModule;
using UnityEngine;

namespace WeaponModule
{
    public class Bullet : MonoBehaviour, IProjectile, IPausable
    {
        [Header("Settings")]
        [SerializeField] private float lifeTime = 3f;
        [SerializeField] private GameObject hitEffect;

        private float _damage;
        private float _remainingLifeTime;
        private Rigidbody _rigidbody;
        private TrailRenderer _trail;
        private Vector3 _pausedVelocity;
        private Vector3 _pausedAngularVelocity;
        private bool _isPaused;
        private bool _hasSetup;

        private void Awake()
        {
            //FIXME: Через SerializeField
            _rigidbody = GetComponent<Rigidbody>();
            _trail = GetComponent<TrailRenderer>();

            if (_trail != null)
                _trail.enabled = false;
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

        public void Setup(float damage, float bulletSpeed, float inheritFactor, Vector3 shooterVelocity)
        {
            _damage = damage;
            _remainingLifeTime = lifeTime;
            _hasSetup = true;

            if (_rigidbody != null)
            {
                _rigidbody.linearVelocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;

                Vector3 bulletVel = transform.forward * bulletSpeed;
                Vector3 playerVel = shooterVelocity * inheritFactor;
                _rigidbody.linearVelocity = bulletVel + playerVel;
            }

            if (_trail != null)
            {
                _trail.Clear();
                _trail.enabled = true;
            }

            SetPaused(PauseState.IsPaused);
        }

        public void SetPaused(bool isPaused)
        {
            if (_isPaused == isPaused)
                return;

            _isPaused = isPaused;

            if (_rigidbody != null)
            {
                if (isPaused)
                {
                    _pausedVelocity = _rigidbody.linearVelocity;
                    _pausedAngularVelocity = _rigidbody.angularVelocity;
                    _rigidbody.linearVelocity = Vector3.zero;
                    _rigidbody.angularVelocity = Vector3.zero;
                    _rigidbody.isKinematic = true;
                }
                else
                {
                    _rigidbody.isKinematic = false;
                    _rigidbody.linearVelocity = _pausedVelocity;
                    _rigidbody.angularVelocity = _pausedAngularVelocity;
                }
            }

            if (_trail != null)
                _trail.emitting = !isPaused;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_isPaused)
                return;

            var entity = collision.gameObject.GetComponentInParent<IEntity>();

            if (entity != null && entity.TryGet<IDamageable>(out var damageable))
            {
                Vector3 force = transform.forward * 10f;
                damageable.TakeDamage(_damage, collision.contacts[0].point, force);
            }

            if (hitEffect != null)
            {
                ContactPoint contact = collision.contacts[0];
                //FIXME: Magic numbers
                GameObject effect = Instantiate(hitEffect, contact.point + contact.normal * 0.05f, Quaternion.LookRotation(contact.normal));
                Destroy(effect, 2f);
            }

            Destroy(gameObject);
        }
    }
}
