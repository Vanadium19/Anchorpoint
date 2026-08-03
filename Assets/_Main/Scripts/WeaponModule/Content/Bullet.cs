using ComponentsModule;
using UnityEngine;

namespace WeaponModule
{
    public class Bullet : MonoBehaviour, IProjectile
    {
        [Header("References")]
        [SerializeField] private Rigidbody rigidbodyComponent;
        [SerializeField] private TrailRenderer trailRenderer;

        [Header("Lifetime")]
        [SerializeField] private float lifeTime = 3f;

        [Header("Hit Feedback")]
        [SerializeField] private GameObject hitEffect;
        [SerializeField] private float hitEffectLifetime = 2f;
        [SerializeField] private AudioSource hitAudioSource;

        private float _damage;

        private void OnValidate()
        {
            rigidbodyComponent ??= GetComponent<Rigidbody>();
            trailRenderer ??= GetComponent<TrailRenderer>();
        }

        public void Setup(float damage, float bulletSpeed, float inheritFactor, Vector3 shooterVelocity)
        {
            _damage = damage;

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

            Destroy(gameObject, lifeTime);
        }

        private void OnCollisionEnter(Collision collision)
        {
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
            PlayHitSound(contact.point);
            Destroy(gameObject);
        }

        private void PlayHitEffect(ContactPoint contact)
        {
            if (hitEffect == null)
                return;

            var position = contact.point + contact.normal * 0.05f;
            var rotation = Quaternion.LookRotation(contact.normal);
            var effect = Instantiate(hitEffect, position, rotation);
            Destroy(effect, hitEffectLifetime);
        }

        private void PlayHitSound(Vector3 position)
        {
            if (hitAudioSource == null || hitAudioSource.clip == null)
                return;

            var audioObject = new GameObject("HitSound");
            audioObject.transform.position = position;

            var audioSource = audioObject.AddComponent<AudioSource>();
            CopyAudioSettings(hitAudioSource, audioSource);
            audioSource.Play();

            Destroy(audioObject, hitAudioSource.clip.length + 0.1f);
        }

        private static void CopyAudioSettings(AudioSource source, AudioSource target)
        {
            target.clip = source.clip;
            target.outputAudioMixerGroup = source.outputAudioMixerGroup;
            target.volume = source.volume;
            target.pitch = source.pitch;
            target.spatialBlend = source.spatialBlend;
            target.rolloffMode = source.rolloffMode;
            target.minDistance = source.minDistance;
            target.maxDistance = source.maxDistance;
            target.playOnAwake = false;
        }
    }
}
