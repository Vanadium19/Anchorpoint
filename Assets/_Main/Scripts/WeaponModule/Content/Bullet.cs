using ComponentsModule;
using UnityEngine;

namespace WeaponModule
{
    public class Bullet : MonoBehaviour, IProjectile
    {
        [Header("Settings")]
        [SerializeField] private float lifeTime = 3f;
        [SerializeField] private GameObject hitEffect;

        private float _damage;
        private Rigidbody _rigidbody;
        private TrailRenderer _trail;
        private GameObject _owner;

        private void Awake()
        {
            //FIXME: Через SerializeField
            _rigidbody = GetComponent<Rigidbody>();
            _trail = GetComponent<TrailRenderer>();

            if (_trail != null)
                _trail.enabled = false;
        }

        public void Setup(float damage, float bulletSpeed, float inheritFactor, Vector3 shooterVelocity, GameObject owner)
        {
            _damage = damage;
            _owner = owner;

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

            Destroy(gameObject, lifeTime);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_owner != null && collision.transform.IsChildOf(_owner.transform))
            {
                Destroy(gameObject);
                return;
            }

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
