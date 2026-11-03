using NUnit.Framework;
using UnityEngine;

namespace WeaponModule.Tests
{
    public class WeaponReticleProjectorTests
    {
        private const float Tolerance = 1e-6f;

        private static readonly Vector2 WindowHalfSize = new(0.0072f, 0.0037f);

        [Test]
        public void TryProject_CameraOnAxis_ReturnsWindowCenter()
        {
            var isProjected = WeaponReticleProjector.TryProject(
                new Vector3(0f, 0f, -0.135f), Vector3.forward, WindowHalfSize, out var position);

            Assert.IsTrue(isProjected);
            Assert.AreEqual(0f, position.x, Tolerance);
            Assert.AreEqual(0f, position.y, Tolerance);
        }

        [Test]
        public void TryProject_OffsetCameraWithForwardAxis_ReturnsCameraOffset()
        {
            var isProjected = WeaponReticleProjector.TryProject(
                new Vector3(0.004f, -0.002f, -0.135f), Vector3.forward, WindowHalfSize, out var position);

            Assert.IsTrue(isProjected);
            Assert.AreEqual(0.004f, position.x, Tolerance);
            Assert.AreEqual(-0.002f, position.y, Tolerance);
        }

        [Test]
        public void TryProject_TiltedAxis_FollowsTheRayThroughThePlane()
        {
            var axis = new Vector3(0.01f, -0.02f, 1f).normalized;
            var camera = new Vector3(0.001f, 0.002f, -0.1f);

            var isProjected = WeaponReticleProjector.TryProject(camera, axis, WindowHalfSize, out var position);

            Assert.IsTrue(isProjected);
            Assert.AreEqual(0.001f + 0.01f * 0.1f, position.x, Tolerance);
            Assert.AreEqual(0.002f - 0.02f * 0.1f, position.y, Tolerance);
        }

        [Test]
        public void TryProject_CrossingOutsideWindow_ReturnsFalse()
        {
            var isProjected = WeaponReticleProjector.TryProject(
                new Vector3(0.02f, 0f, -0.135f), Vector3.forward, WindowHalfSize, out var position);

            Assert.IsFalse(isProjected);
            Assert.AreEqual(Vector2.zero, position);
        }

        [Test]
        public void TryProject_CrossingOutsideWindowVertically_ReturnsFalse()
        {
            var isProjected = WeaponReticleProjector.TryProject(
                new Vector3(0f, 0.005f, -0.135f), Vector3.forward, WindowHalfSize, out _);

            Assert.IsFalse(isProjected);
        }

        [Test]
        public void TryProject_CameraBehindThePlaneSide_ReturnsFalse()
        {
            Assert.IsFalse(WeaponReticleProjector.TryProject(
                new Vector3(0f, 0f, 0.05f), Vector3.forward, WindowHalfSize, out _));
            Assert.IsFalse(WeaponReticleProjector.TryProject(
                Vector3.zero, Vector3.forward, WindowHalfSize, out _));
        }

        [Test]
        public void TryProject_AxisNotLookingForward_ReturnsFalse()
        {
            var camera = new Vector3(0f, 0f, -0.135f);

            Assert.IsFalse(WeaponReticleProjector.TryProject(camera, Vector3.back, WindowHalfSize, out _));
            Assert.IsFalse(WeaponReticleProjector.TryProject(camera, Vector3.right, WindowHalfSize, out _));
        }

        [Test]
        public void TryProjectClamped_CrossingInsideWindow_MatchesTryProject()
        {
            var camera = new Vector3(0.004f, -0.002f, -0.135f);

            var isProjected = WeaponReticleProjector.TryProjectClamped(
                camera, Vector3.forward, WindowHalfSize, out var clamped);

            WeaponReticleProjector.TryProject(camera, Vector3.forward, WindowHalfSize, out var exact);

            Assert.IsTrue(isProjected);
            Assert.AreEqual(exact, clamped);
        }

        [Test]
        public void TryProjectClamped_CrossingOutsideWindow_HoldsTheDotAtTheEdge()
        {
            var isProjected = WeaponReticleProjector.TryProjectClamped(
                new Vector3(0.02f, -0.01f, -0.135f), Vector3.forward, WindowHalfSize, out var position);

            Assert.IsTrue(isProjected);
            Assert.AreEqual(WindowHalfSize.x, position.x, Tolerance);
            Assert.AreEqual(-WindowHalfSize.y, position.y, Tolerance);
        }

        [Test]
        public void TryProjectClamped_RecoilTiltedAxisFarOutside_StaysInsideWindow()
        {
            var axis = new Vector3(-0.3f, 0.4f, 1f).normalized;

            var isProjected = WeaponReticleProjector.TryProjectClamped(
                new Vector3(0f, 0f, -0.135f), axis, WindowHalfSize, out var position);

            Assert.IsTrue(isProjected);
            Assert.AreEqual(-WindowHalfSize.x, position.x, Tolerance);
            Assert.AreEqual(WindowHalfSize.y, position.y, Tolerance);
        }

        [Test]
        public void TryProjectClamped_CameraBehindThePlaneSideOrAxisNotForward_ReturnsFalse()
        {
            Assert.IsFalse(WeaponReticleProjector.TryProjectClamped(
                new Vector3(0f, 0f, 0.05f), Vector3.forward, WindowHalfSize, out var position));
            Assert.AreEqual(Vector2.zero, position);
            Assert.IsFalse(WeaponReticleProjector.TryProjectClamped(
                new Vector3(0f, 0f, -0.135f), Vector3.right, WindowHalfSize, out _));
        }

        [Test]
        public void ClampToWindow_PointInsideWindow_IsUnchanged()
        {
            var position = new Vector2(0.003f, -0.001f);

            Assert.AreEqual(position, WeaponReticleProjector.ClampToWindow(position, WindowHalfSize));
        }

        [Test]
        public void ClampToWindow_PointOutsideOnOneAxis_ClampsOnlyThatAxis()
        {
            var clamped = WeaponReticleProjector.ClampToWindow(new Vector2(0.001f, 0.05f), WindowHalfSize);

            Assert.AreEqual(0.001f, clamped.x, Tolerance);
            Assert.AreEqual(WindowHalfSize.y, clamped.y, Tolerance);
        }
    }
}
