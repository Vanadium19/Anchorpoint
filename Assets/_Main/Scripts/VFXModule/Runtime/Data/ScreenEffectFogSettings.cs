using UnityEngine;

namespace VFXModule
{
    /// <summary>
    /// Optional distance-fog target carried on a screen-effect prefab, blended in alongside the effect's own weight.
    /// </summary>
    /// <remarks>
    /// Lives next to the <see cref="UnityEngine.Rendering.Volume"/> component on the effect's own prefab, so the fog look is
    /// data owned by that specific effect rather than a setting on the generic VFX installer. A prefab without this component
    /// gets no fog blending.
    /// </remarks>
    public class ScreenEffectFogSettings : MonoBehaviour
    {
        [SerializeField] private Color fogColor = Color.gray;
        [SerializeField] [Min(0f)] private float fogDensity;

        /// <summary>Fog color to blend toward at full intensity.</summary>
        public Color FogColor => fogColor;

        /// <summary>Fog density to blend toward at full intensity.</summary>
        public float FogDensity => fogDensity;
    }
}
