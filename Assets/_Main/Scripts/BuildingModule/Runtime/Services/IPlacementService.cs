using UnityEngine;

namespace BuildingModule
{
    public interface IPlacementService
    {
        BuildingName CurrentBuilding { get; }

        void SetBuilding(BuildingName value);
        void UpdatePosition(Vector3 worldPosition);
        bool Build(Vector3 worldPosition);
        void Cancel();
    }
}