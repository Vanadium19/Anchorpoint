using UnityEngine;

namespace WeaponModule
{
    public class CameraRecoilService : ICameraRecoilService
    {
        private readonly RecoilProcessor _processor = new();

        public Vector3 CurrentRotation => _processor.CurrentRotation;

        public void Update(float deltaTime) => _processor.Update(deltaTime);

        public void FireCamera(CameraRecoilSettings settings) => _processor.FireCamera(settings);
    }
}
