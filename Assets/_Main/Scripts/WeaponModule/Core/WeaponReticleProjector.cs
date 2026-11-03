using UnityEngine;

namespace WeaponModule
{
    /// <summary>
    /// Finds where the dot of a collimated sight appears in the window of the sight. Pure math over Unity value
    /// types: no scene access, callable from EditMode tests.
    /// </summary>
    public static class WeaponReticleProjector
    {
        private const float MinAxisForward = 1e-6f;

        /// <summary>
        /// Projects the dot onto the window. All values are in the local space of the aim point of the sight, the
        /// window plane is local z = 0 and the shooter looks through it towards +z. A collimated dot is at infinity,
        /// so it appears where the ray from <paramref name="cameraPosition"/> along <paramref name="sightAxis"/>
        /// crosses the window plane. Returns false, and the dot is hidden, when the camera is not behind the window
        /// looking forward or when the crossing point is outside <paramref name="windowHalfSize"/>.
        /// </summary>
        public static bool TryProject(
            Vector3 cameraPosition,
            Vector3 sightAxis,
            Vector2 windowHalfSize,
            out Vector2 reticlePosition)
        {
            reticlePosition = Vector2.zero;

            if (!TryFindCrossing(cameraPosition, sightAxis, out var crossing))
                return false;

            if (Mathf.Abs(crossing.x) > windowHalfSize.x || Mathf.Abs(crossing.y) > windowHalfSize.y)
                return false;

            reticlePosition = crossing;

            return true;
        }

        /// <summary>
        /// Same projection as <see cref="TryProject"/>, but a crossing outside <paramref name="windowHalfSize"/> is
        /// moved to the nearest point of the window edge instead of hiding the dot. Returns false only when the camera
        /// is not behind the window looking forward.
        /// </summary>
        public static bool TryProjectClamped(
            Vector3 cameraPosition,
            Vector3 sightAxis,
            Vector2 windowHalfSize,
            out Vector2 reticlePosition)
        {
            reticlePosition = Vector2.zero;

            if (!TryFindCrossing(cameraPosition, sightAxis, out var crossing))
                return false;

            reticlePosition = ClampToWindow(crossing, windowHalfSize);

            return true;
        }

        /// <summary>
        /// Clamps a point of the window plane to the rectangle of <paramref name="windowHalfSize"/> around the center.
        /// </summary>
        public static Vector2 ClampToWindow(Vector2 position, Vector2 windowHalfSize) => new(
            Mathf.Clamp(position.x, -windowHalfSize.x, windowHalfSize.x),
            Mathf.Clamp(position.y, -windowHalfSize.y, windowHalfSize.y));

        private static bool TryFindCrossing(Vector3 cameraPosition, Vector3 sightAxis, out Vector2 crossing)
        {
            crossing = Vector2.zero;

            if (sightAxis.z <= MinAxisForward || cameraPosition.z >= 0f)
                return false;

            var distance = -cameraPosition.z / sightAxis.z;

            crossing = new Vector2(
                cameraPosition.x + sightAxis.x * distance,
                cameraPosition.y + sightAxis.y * distance);

            return true;
        }
    }
}
