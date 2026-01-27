using ComponentsModule;
using UnityEngine;

namespace WeaponModule
{
    public class Bullet : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float lifeTime = 3f;
        [SerializeField] private GameObject hitEffect;

        private float _damage;
        private Rigidbody _rigidbody;
        private TrailRenderer _trail;

        private void Awake()
        {
            //FIXME: Через SerializeField
            _rigidbody = GetComponent<Rigidbody>();
            _trail = GetComponent<TrailRenderer>();

            if (_trail != null)
                _trail.enabled = false;
        }

        public void Setup(float damage, float bulletSpeed, float inheritFactor, Vector3 shooterVelocity)
        {
            _damage = damage;

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
            //TODO: Через прокси
            var target = collision.gameObject.GetComponentInParent<IDamageable>();

            if (target != null)
            {
                Vector3 force = transform.forward * 10f;
                target.TakeDamage(_damage, collision.contacts[0].point, force);
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