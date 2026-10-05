using System;
using UnityEngine;

namespace BuildingModule
{
    public sealed class MinefieldView : MonoBehaviour
    {
        [SerializeField] private Collider triggerCollider;
        [SerializeField] private GameObject[] mineVisuals;
        [SerializeField] private GameObject explosionEffect;
        [SerializeField] [Min(0.1f)] private float explosionEffectLifetime = 2f;
        [SerializeField] private AudioSource explosionAudioSource;

        public event Action<Collider> TargetEntered;
        public event Action<Collider> TargetExited;

        private void OnTriggerEnter(Collider other) => TargetEntered?.Invoke(other);

        private void OnTriggerExit(Collider other) => TargetExited?.Invoke(other);

        public void SetTriggerEnabled(bool isEnabled)
        {
            if (triggerCollider != null)
                triggerCollider.enabled = isEnabled;
        }

        public bool TryConsumeNearestMine(Vector3 targetPosition, out Vector3 minePosition)
        {
            minePosition = targetPosition;

            if (mineVisuals == null || mineVisuals.Length == 0)
                return false;

            GameObject nearestMine = null;
            var nearestSquaredDistance = float.MaxValue;

            foreach (var mineVisual in mineVisuals)
            {
                if (mineVisual == null || !mineVisual.activeSelf)
                    continue;

                var squaredDistance =
                    (mineVisual.transform.position - targetPosition).sqrMagnitude;

                if (squaredDistance >= nearestSquaredDistance)
                    continue;

                nearestSquaredDistance = squaredDistance;
                nearestMine = mineVisual;
            }

            if (nearestMine == null)
                return false;

            minePosition = nearestMine.transform.position;
            nearestMine.SetActive(false);

            return true;
        }

        public void PlayExplosion(Vector3 position)
        {
            PlayExplosionEffect(position);
            PlayExplosionSound(position);
        }

        private void PlayExplosionEffect(Vector3 position)
        {
            if (explosionEffect == null)
                return;

            var effect = Instantiate(explosionEffect, position, Quaternion.identity);
            Destroy(effect, explosionEffectLifetime);
        }

        private void PlayExplosionSound(Vector3 position)
        {
            if (explosionAudioSource == null || explosionAudioSource.clip == null)
                return;

            var audioObject = new GameObject("MineExplosionSound");
            audioObject.transform.position = position;

            var audioSource = audioObject.AddComponent<AudioSource>();
            CopyAudioSettings(explosionAudioSource, audioSource);
            audioSource.Play();

            Destroy(audioObject, explosionAudioSource.clip.length + 0.1f);
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
