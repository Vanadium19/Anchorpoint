using UnityEngine;

namespace BuildingModule
{
    public interface IPreviewService
    {
        void SetPreview(BuildingName value);
        void UpdatePreview(Vector3 position, bool isOccupied);
        void Cancel();
    }
}