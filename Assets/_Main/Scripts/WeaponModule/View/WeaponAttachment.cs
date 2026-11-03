using UnityEngine;

namespace WeaponModule
{
    /// <summary>
    /// Root component of a weapon attachment prefab (sight, magazine, barrel). The origin of the root is the
    /// contact point with the mount of the weapon socket, local +Z points forward along the bore, +Y is up.
    /// </summary>
    public class WeaponAttachment : MonoBehaviour
    {
        [Tooltip("The center of the window of a sight. Leave empty for attachments that do not change the sight line.")]
        [SerializeField] private Transform aimPoint;
        [Tooltip("The dot mesh of a collimated sight: a child of the aim point that is moved inside the window.")]
        [SerializeField] private Transform reticle;
        [Tooltip("Half size of the area of the window, in aim point units, where the dot center may be.")]
        [SerializeField] private Vector2 reticleWindowHalfSize = new(0.007f, 0.0035f);

        public Transform AimPoint => aimPoint;
        public Transform Reticle => reticle;
        public Vector2 ReticleWindowHalfSize => reticleWindowHalfSize;

        /// <summary>
        /// Moves the dot to where a ray from the camera along the sight axis crosses the window of the sight, as a
        /// collimated dot at infinity appears. The dot is hidden while the ray misses the window, except while
        /// <paramref name="isAimed"/>: then it stays visible and is held at the edge of the window.
        /// </summary>
        public void UpdateReticle(Vector3 cameraWorldPosition, Vector3 sightAxisWorld, bool isAimed)
        {
            if (aimPoint == null || reticle == null || !isActiveAndEnabled)
                return;

            var cameraPosition = aimPoint.InverseTransformPoint(cameraWorldPosition);
            var sightAxis = aimPoint.InverseTransformVector(sightAxisWorld);
            var isVisible = isAimed
                ? WeaponReticleProjector.TryProjectClamped(
                    cameraPosition, sightAxis, reticleWindowHalfSize, out var reticlePosition)
                : WeaponReticleProjector.TryProject(
                    cameraPosition, sightAxis, reticleWindowHalfSize, out reticlePosition);

            if (reticle.gameObject.activeSelf != isVisible)
                reticle.gameObject.SetActive(isVisible);

            if (isVisible)
                reticle.localPosition = new Vector3(reticlePosition.x, reticlePosition.y, 0f);
        }
    }
}
