using UnityEngine;

namespace BuildingModule
{
    public interface IPreviewService
    {
        void SetPreview(string id);
        void UpdatePreview(Vector3 position, bool isOccupied);
        void UpdatePreview(Vector3 position, bool isOccupied, float rotation);
        void UpdatePreview(Vector3 position, bool isOccupied, bool hasGroundSupport);
        void UpdatePreview(float rotation);
        void Cancel();
        bool HasCollisionAtPosition(Vector3 position, float rotation);
        bool TryGetCollisionBounds(Vector3 position, float rotation, out Vector3 worldCenter, out Vector3 halfSize, out Quaternion totalRotation);
    }
}