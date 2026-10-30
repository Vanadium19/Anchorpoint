using UnityEngine;

namespace WeaponModule
{
    public class CameraZoomService : ICameraZoomService
    {
        private const float MinZoom = 1f;

        private Camera _camera;
        private float _baseFieldOfView;

        public void BlendZoom(float zoom, float blend)
        {
            if (!TryGetCamera())
                return;

            var targetFieldOfView = _baseFieldOfView / Mathf.Max(MinZoom, zoom);
            _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, targetFieldOfView, Mathf.Clamp01(blend));
        }

        public void ResetZoom()
        {
            if (TryGetCamera())
                _camera.fieldOfView = _baseFieldOfView;
        }

        private bool TryGetCamera()
        {
            if (_camera != null)
                return true;

            _camera = Camera.main;

            if (_camera == null)
                return false;

            _baseFieldOfView = _camera.fieldOfView;
            return true;
        }
    }
}
