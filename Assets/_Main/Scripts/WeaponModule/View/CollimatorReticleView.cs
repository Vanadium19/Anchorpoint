using UnityEngine;
using UnityEngine.Rendering;

namespace WeaponModule
{
    /// <summary>
    /// Makes the reticle of a reflex sight collimated: before the camera renders, the reticle is moved inside
    /// the sight window onto the line from the eye to a far point along the sight axis, so it shows where the
    /// barrel points regardless of where the eye is (recoil, sway). Hidden when the eye is outside the window.
    /// </summary>
    public class CollimatorReticleView : WeaponViewPart
    {
        private const float CollimationDistance = 100f;

        [Tooltip("Reticle rest point in the window centre; forward points along the barrel.")]
        [SerializeField] private Transform sightAxis;
        [SerializeField] private Transform reticle;
        [Tooltip("Largest reticle shift from the window centre in metres before it leaves the window.")]
        [Min(0f)]
        [SerializeField] private float maxOffset = 0.004f;

        private Camera _camera;

        private void OnEnable() => RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;

        private void OnDisable() => RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;

        /// <inheritdoc/>
        public override void Initialize(WeaponConfig config) => _camera = Camera.main;

        private void OnBeginCameraRendering(ScriptableRenderContext context, Camera renderingCamera)
        {
            if (renderingCamera != _camera || sightAxis == null || reticle == null)
                return;

            var eye = renderingCamera.transform.position;
            var axisOrigin = sightAxis.position;
            var axisDirection = sightAxis.forward;
            var lineDirection = axisOrigin + axisDirection * CollimationDistance - eye;
            var denominator = Vector3.Dot(lineDirection, axisDirection);

            if (denominator <= Mathf.Epsilon)
                return;

            var distance = Vector3.Dot(axisOrigin - eye, axisDirection) / denominator;
            var offset = eye + lineDirection * distance - axisOrigin;
            var isInsideWindow = offset.magnitude <= maxOffset;

            reticle.gameObject.SetActive(isInsideWindow);

            if (isInsideWindow)
                reticle.position = axisOrigin + offset;
        }
    }
}
