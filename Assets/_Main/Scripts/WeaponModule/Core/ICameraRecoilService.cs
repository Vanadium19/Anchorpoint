using UnityEngine;

namespace WeaponModule
{
    public interface ICameraRecoilService
    {
        Vector3 CurrentRotation { get; }

        void Update(float deltaTime);
        void FireCamera(CameraRecoilSettings settings);
    }
}
