using System;
using UnityEngine;

namespace WeaponModule
{
    /// <summary>
    /// Muzzle flash of a weapon: one effect instance at the muzzle that plays once per shot.
    /// </summary>
    public class MuzzleFlashView : WeaponViewPart
    {
        [SerializeField] private GameObject flashPrefab;
        [SerializeField] private Transform muzzle;
        [Tooltip("World size of the flash, independent of the scale of the muzzle bone.")]
        [SerializeField, Min(0.001f)] private float flashSize = 0.5f;
        [Tooltip("Brightness of the flash, scales the opacity of every layer of the effect.")]
        [SerializeField, Range(0f, 1f)] private float flashIntensity = 1f;

        private ParticleSystem[] _particles = Array.Empty<ParticleSystem>();
        private Transform _flash;

        public override void Initialize(WeaponConfig config)
        {
            if (_flash != null || flashPrefab == null || muzzle == null)
                return;

            var flash = Instantiate(flashPrefab, muzzle);
            _flash = flash.transform;
            _flash.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            _flash.localScale = Vector3.one * (flashSize / muzzle.lossyScale.x);
            SetLayer(_flash, muzzle.gameObject.layer);
            _particles = flash.GetComponentsInChildren<ParticleSystem>();
            ApplyIntensity();
        }

        public override void OnFired()
        {
            if (_flash == null)
                return;

            _flash.localRotation = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 360f));

            foreach (var particles in _particles)
                particles.Play();
        }

        public override void ResetVisuals()
        {
            foreach (var particles in _particles)
                particles.Clear();
        }

        private void ApplyIntensity()
        {
            foreach (var particles in _particles)
            {
                var main = particles.main;
                main.startColor = new Color(1f, 1f, 1f, flashIntensity);
            }
        }

        private static void SetLayer(Transform root, int layer)
        {
            foreach (var child in root.GetComponentsInChildren<Transform>(true))
                child.gameObject.layer = layer;
        }
    }
}
