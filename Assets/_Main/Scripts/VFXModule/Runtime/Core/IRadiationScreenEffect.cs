namespace VFXModule
{
    /// <summary>
    /// Full-screen post-process tint used while the player is exposed to radiation.
    /// </summary>
    /// <remarks>
    /// A missing binding is a valid no-op, so callers do not need to guard against a scene without the effect wired up.
    /// </remarks>
    public interface IRadiationScreenEffect
    {
        /// <summary>
        /// Sets how strongly the tint is blended in, from 0 (invisible) to 1 (full strength).
        /// </summary>
        void SetIntensity(float normalizedIntensity);
    }
}
