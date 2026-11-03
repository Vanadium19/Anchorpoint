using UnityEngine;

namespace WeaponModule
{
    /// <summary>
    /// Derives the pose of the aim pivot that puts the sight line of a weapon on the view axis.
    /// Pure math over Unity value types: no scene access, callable from EditMode tests.
    /// </summary>
    public static class WeaponAimPoseSolver
    {
        private const float MinSightLengthSquared = 1e-8f;
        private const float NearlyOppositeDot = -0.999f;

        /// <summary>
        /// Solves the local pose of the aim pivot. <paramref name="aimPoint"/>, <paramref name="frontSight"/> and
        /// <paramref name="fallbackForward"/> are in the local space of the aim pivot at rest. The view values describe
        /// the camera in the parent space of the aim pivot. The sight line runs from <paramref name="aimPoint"/> to
        /// <paramref name="frontSight"/>, or along <paramref name="fallbackForward"/> when there is no front sight.
        /// After the solve the sight line is parallel to <paramref name="viewForward"/> and
        /// <paramref name="aimPoint"/> lies on the view axis at <paramref name="eyeRelief"/> from the camera.
        /// The rotation is the minimal rotation from <paramref name="baseRotation"/> that aligns the sight line,
        /// so the roll of the authored aim rotation is kept. <paramref name="viewUp"/> only picks the turn axis
        /// when the sight line points straight away from the view direction.
        /// </summary>
        public static void Solve(
            Vector3 aimPoint,
            Vector3 frontSight,
            Vector3 fallbackForward,
            bool hasFrontSight,
            Vector3 viewOrigin,
            Vector3 viewForward,
            Vector3 viewUp,
            Quaternion baseRotation,
            float eyeRelief,
            out Vector3 position,
            out Quaternion rotation)
        {
            var viewAxis = NormalizeOr(viewForward, Vector3.forward);
            var sightLine = hasFrontSight ? frontSight - aimPoint : Vector3.zero;

            if (sightLine.sqrMagnitude < MinSightLengthSquared)
                sightLine = fallbackForward;

            var sightDirection = NormalizeOr(sightLine, Vector3.forward);
            var currentDirection = baseRotation * sightDirection;

            rotation = GetAlignment(currentDirection, viewAxis, viewUp) * baseRotation;
            position = viewOrigin + viewAxis * eyeRelief - rotation * aimPoint;
        }

        private static Quaternion GetAlignment(Vector3 from, Vector3 to, Vector3 up)
        {
            if (Vector3.Dot(from, to) > NearlyOppositeDot)
                return Quaternion.FromToRotation(from, to);

            var axis = Vector3.ProjectOnPlane(up, to);

            if (axis.sqrMagnitude < MinSightLengthSquared)
                axis = Vector3.Cross(to, Vector3.right);

            if (axis.sqrMagnitude < MinSightLengthSquared)
                axis = Vector3.Cross(to, Vector3.up);

            var halfTurn = Quaternion.AngleAxis(180f, axis.normalized);

            return Quaternion.FromToRotation(halfTurn * from, to) * halfTurn;
        }

        private static Vector3 NormalizeOr(Vector3 vector, Vector3 fallback) =>
            vector.sqrMagnitude < MinSightLengthSquared ? fallback : vector.normalized;
    }
}
