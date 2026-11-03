using NUnit.Framework;
using UnityEngine;

namespace WeaponModule.Tests
{
    public class WeaponAimPoseSolverTests
    {
        private const float PositionTolerance = 1e-4f;
        private const float AngleTolerance = 0.01f;

        private static readonly Vector3 AimPoint = new(0.01f, 0.08f, 0.12f);
        private static readonly Vector3 FrontSight = new(0.012f, 0.095f, 0.62f);
        private static readonly Vector3 ViewOrigin = new(0.02f, 0.01f, -0.03f);

        [Test]
        public void Solve_AlignedInputs_PutsAimPointAndFrontSightOnViewAxis()
        {
            WeaponAimPoseSolver.Solve(AimPoint, FrontSight, Vector3.forward, true, ViewOrigin, Vector3.forward,
                Vector3.up, Quaternion.identity, 0.3f, out var position, out var rotation);

            AssertSightLineOnAxis(position, rotation, ViewOrigin, Vector3.forward, 0.3f, AimPoint, FrontSight);
        }

        [Test]
        public void Solve_TiltedAndOffsetInputs_PutsAimPointAndFrontSightOnViewAxis()
        {
            var viewForward = (Quaternion.Euler(7f, -12f, 0f) * Vector3.forward).normalized;
            var viewUp = Quaternion.Euler(7f, -12f, 0f) * Vector3.up;
            var viewOrigin = new Vector3(-0.4f, 0.25f, 0.7f);
            var baseRotation = Quaternion.Euler(3f, -5f, 4f);

            WeaponAimPoseSolver.Solve(AimPoint, FrontSight, Vector3.forward, true, viewOrigin, viewForward, viewUp,
                baseRotation, 0.22f, out var position, out var rotation);

            AssertSightLineOnAxis(position, rotation, viewOrigin, viewForward, 0.22f, AimPoint, FrontSight);
        }

        [Test]
        public void Solve_NonNormalizedViewForward_StillAlignsToViewAxis()
        {
            var viewForward = new Vector3(0.1f, -0.05f, 3f);

            WeaponAimPoseSolver.Solve(AimPoint, FrontSight, Vector3.forward, true, ViewOrigin, viewForward,
                Vector3.up, Quaternion.identity, 0.3f, out var position, out var rotation);

            AssertSightLineOnAxis(position, rotation, ViewOrigin, viewForward, 0.3f, AimPoint, FrontSight);
        }

        [Test]
        public void Solve_WithoutFrontSight_UsesFallbackForward()
        {
            var fallbackForward = (Quaternion.Euler(-2f, 3f, 0f) * Vector3.forward).normalized;
            var viewForward = (Quaternion.Euler(5f, 20f, 0f) * Vector3.forward).normalized;

            WeaponAimPoseSolver.Solve(AimPoint, Vector3.zero, fallbackForward, false, ViewOrigin, viewForward,
                Vector3.up, Quaternion.identity, 0.25f, out var position, out var rotation);

            var pointAhead = AimPoint + fallbackForward * 0.5f;

            AssertSightLineOnAxis(position, rotation, ViewOrigin, viewForward, 0.25f, AimPoint, pointAhead);
        }

        [Test]
        public void Solve_FrontSightOnAimPoint_UsesFallbackForward()
        {
            var fallbackForward = (Quaternion.Euler(4f, -6f, 0f) * Vector3.forward).normalized;

            WeaponAimPoseSolver.Solve(AimPoint, AimPoint, fallbackForward, true, ViewOrigin, Vector3.forward,
                Vector3.up, Quaternion.identity, 0.3f, out var position, out var rotation);

            var pointAhead = AimPoint + fallbackForward * 0.5f;

            AssertSightLineOnAxis(position, rotation, ViewOrigin, Vector3.forward, 0.3f, AimPoint, pointAhead);
        }

        [Test]
        public void Solve_RollStaysMinimalFromBaseRotation()
        {
            var baseRotation = Quaternion.Euler(2f, -3f, 9f);
            var viewForward = (Quaternion.Euler(-4f, 10f, 0f) * Vector3.forward).normalized;
            var sightDirection = (FrontSight - AimPoint).normalized;

            WeaponAimPoseSolver.Solve(AimPoint, FrontSight, Vector3.forward, true, ViewOrigin, viewForward,
                Vector3.up, baseRotation, 0.3f, out _, out var rotation);

            var minimalAngle = Vector3.Angle(baseRotation * sightDirection, viewForward);
            var appliedAngle = Quaternion.Angle(baseRotation, rotation);

            Assert.AreEqual(minimalAngle, appliedAngle, AngleTolerance);
        }

        [Test]
        public void Solve_LevelView_KeepsWeaponUpCloseToViewUp()
        {
            var viewForward = (Quaternion.Euler(3f, 8f, 0f) * Vector3.forward).normalized;
            var viewUp = Quaternion.Euler(3f, 8f, 0f) * Vector3.up;

            WeaponAimPoseSolver.Solve(AimPoint, FrontSight, Vector3.forward, true, ViewOrigin, viewForward, viewUp,
                Quaternion.identity, 0.3f, out _, out var rotation);

            Assert.Less(Vector3.Angle(rotation * Vector3.up, viewUp), 5f);
        }

        [Test]
        public void Solve_SightLineOppositeToView_StillAligns()
        {
            var backwardSight = new Vector3(0.01f, 0.08f, -0.5f);

            WeaponAimPoseSolver.Solve(AimPoint, backwardSight, Vector3.forward, true, ViewOrigin, Vector3.forward,
                Vector3.up, Quaternion.identity, 0.3f, out var position, out var rotation);

            AssertSightLineOnAxis(position, rotation, ViewOrigin, Vector3.forward, 0.3f, AimPoint, backwardSight);
            Assert.Greater(Vector3.Dot(rotation * Vector3.up, Vector3.up), 0f);
        }

        [Test]
        public void Solve_RandomInputs_AlwaysLandOnViewAxis()
        {
            var random = new System.Random(1234);

            for (var i = 0; i < 200; i++)
            {
                var aimPoint = RandomVector(random, 0.2f);
                var frontSight = aimPoint + RandomDirection(random) * (0.3f + (float)random.NextDouble() * 0.5f);
                var viewForward = RandomDirection(random);
                var viewUp = Vector3.ProjectOnPlane(RandomDirection(random), viewForward);
                var viewOrigin = RandomVector(random, 1f);
                var baseRotation = Quaternion.Euler(RandomVector(random, 20f));
                var eyeRelief = 0.1f + (float)random.NextDouble() * 0.4f;

                WeaponAimPoseSolver.Solve(aimPoint, frontSight, Vector3.forward, true, viewOrigin, viewForward,
                    viewUp, baseRotation, eyeRelief, out var position, out var rotation);

                AssertSightLineOnAxis(position, rotation, viewOrigin, viewForward, eyeRelief, aimPoint, frontSight);
            }
        }

        private static Vector3 Transform(Vector3 position, Quaternion rotation, Vector3 point) =>
            position + rotation * point;

        private static float DistanceToAxis(Vector3 point, Vector3 origin, Vector3 forward) =>
            Vector3.Cross(point - origin, forward.normalized).magnitude;

        private static Vector3 RandomVector(System.Random random, float range) => new(
            ((float)random.NextDouble() * 2f - 1f) * range,
            ((float)random.NextDouble() * 2f - 1f) * range,
            ((float)random.NextDouble() * 2f - 1f) * range);

        private static Vector3 RandomDirection(System.Random random)
        {
            var direction = RandomVector(random, 1f);

            return direction.sqrMagnitude < 0.01f ? Vector3.forward : direction.normalized;
        }

        private static void AssertSightLineOnAxis(Vector3 position, Quaternion rotation, Vector3 viewOrigin,
            Vector3 viewForward, float eyeRelief, Vector3 aimPoint, Vector3 frontSight)
        {
            var axis = viewForward.normalized;
            var aimPointInView = Transform(position, rotation, aimPoint);
            var frontSightInView = Transform(position, rotation, frontSight);

            Assert.Less(DistanceToAxis(aimPointInView, viewOrigin, axis), PositionTolerance);
            Assert.Less(DistanceToAxis(frontSightInView, viewOrigin, axis), PositionTolerance);
            Assert.AreEqual(eyeRelief, Vector3.Dot(aimPointInView - viewOrigin, axis), PositionTolerance);
            Assert.Greater(Vector3.Dot(frontSightInView - aimPointInView, axis), 0f);
        }
    }
}
