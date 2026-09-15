using UnityEngine;
using UnityEngine.Rendering;

namespace VFXModule
{
    /// <summary>
    /// Default <see cref="IRadiationScreenEffect"/>, driving a lazily spawned <see cref="Volume"/>'s weight and the scene's distance fog.
    /// </summary>
    /// <remarks>
    /// The volume prefab comes from <see cref="ScreenEffectsCatalog"/> (keyed by <see cref="ScreenEffectId.Radiation"/>) and is
    /// instantiated on first use, the same way <see cref="IEffectsService"/> spawns particle effects on demand, so no
    /// <see cref="Volume"/> sits in the scene until radiation is actually active. Its profile carries the tint (vignette, color
    /// grading, chromatic aberration); this class fades that in and out. If the prefab also carries a
    /// <see cref="ScreenEffectFogSettings"/>, its color/density are blended into <see cref="RenderSettings.fogColor"/>/
    /// <see cref="RenderSettings.fogDensity"/> alongside the tint, restoring the scene's own fog settings (captured at
    /// construction) once the intensity returns to 0 — a prefab without that component gets no fog blending.
    /// </remarks>
    public class RadiationScreenEffect : IRadiationScreenEffect
    {
        private readonly Volume _volumePrefab;
        private readonly Transform _container;

        private readonly bool _baselineFogEnabled;
        private readonly Color _baselineFogColor;
        private readonly float _baselineFogDensity;

        private Volume _volumeInstance;
        private ScreenEffectFogSettings _fogSettings;

        /// <summary>Creates the effect over the catalog's radiation volume prefab and its spawn container; a missing catalog or prefab makes it a no-op.</summary>
        public RadiationScreenEffect(ScreenEffectsCatalog catalog, Transform container)
        {
            _volumePrefab = catalog != null ? catalog.GetPrefab(ScreenEffectId.Radiation) : null;
            _container = container;

            _baselineFogEnabled = RenderSettings.fog;
            _baselineFogColor = RenderSettings.fogColor;
            _baselineFogDensity = RenderSettings.fogDensity;
        }

        /// <inheritdoc/>
        public void SetIntensity(float normalizedIntensity)
        {
            var intensity = Mathf.Clamp01(normalizedIntensity);

            if (_volumeInstance == null && intensity > 0f && _volumePrefab != null)
            {
                _volumeInstance = Object.Instantiate(_volumePrefab, _container);
                _volumeInstance.TryGetComponent(out _fogSettings);
            }

            if (_volumeInstance != null)
                _volumeInstance.weight = intensity;

            if (_fogSettings != null)
            {
                RenderSettings.fogMode = FogMode.ExponentialSquared;
                RenderSettings.fog = _baselineFogEnabled || intensity > 0f;
                RenderSettings.fogColor = Color.Lerp(_baselineFogColor, _fogSettings.FogColor, intensity);
                RenderSettings.fogDensity = Mathf.Lerp(_baselineFogDensity, _fogSettings.FogDensity, intensity);
            }
        }
    }
}
