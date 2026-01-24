using UnityEngine;

namespace CameraModule
{
    public interface IBuildingModeCameraService
    {
        void Move(Vector3 direction);
        void Rotate(float delta);
        void Reset();
    }
}