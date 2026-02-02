using System;
using BuildingModule;
using UnityEngine;
using Zenject;

namespace CameraModule
{
    public class BuildingModeCameraController : IInitializable, ILateTickable, IDisposable
    {
        private readonly IBuildingModeCameraService _cameraService;
        private readonly IConstructionModeService _modeService;

        private bool _isActive;

        public BuildingModeCameraController(IConstructionModeService modeService, IBuildingModeCameraService cameraService)
        {
            _modeService = modeService;
            _cameraService = cameraService;
        }

        public void Initialize() => _modeService.ActiveChanged += SetActive;

        public void LateTick()
        {
            if (!_isActive)
                return;

            Move();
            Rotate();
        }

        public void Dispose() => _modeService.ActiveChanged -= SetActive;

        public void SetActive(bool value)
        {
            if (_isActive == value) return;

            if (value)
            {
                _cameraService.CaptureStartPosition();
            }

            _isActive = value;

            if (!_isActive)
                _cameraService.Reset();
        }

        private void Move()
        {
            var direction = Vector3.right * Input.GetAxis("Horizontal") + Vector3.forward * Input.GetAxis("Vertical");
            _cameraService.Move(direction);
        }

        private void Rotate()
        {
            if (!Input.GetMouseButton(2))
                return;

            var delta = Input.GetAxisRaw("Mouse X");
            _cameraService.Rotate(delta);
        }
    }
}