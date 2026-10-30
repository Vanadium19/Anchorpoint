namespace WeaponModule
{
    /// <summary>
    /// Zooms the player camera while aiming. The base field of view is shared by all weapons.
    /// </summary>
    public interface ICameraZoomService
    {
        /// <summary>
        /// Moves the field of view towards the base value divided by <paramref name="zoom"/>.
        /// </summary>
        /// <param name="zoom">Magnification, 1 restores the base field of view.</param>
        /// <param name="blend">Interpolation factor of this frame in the 0..1 range.</param>
        void BlendZoom(float zoom, float blend);

        void ResetZoom();
    }
}
