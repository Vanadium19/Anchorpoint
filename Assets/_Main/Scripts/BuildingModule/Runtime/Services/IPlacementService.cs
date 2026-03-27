using System;
using UnityEngine;

namespace BuildingModule
{
    public interface IPlacementService
    {
        event Action<string> BuildingChanged;
        event Action PlacementCompleted;
        event Action SelectionCleared;

        string CurrentBuildingId { get; }
        Vector3 LastValidPosition { get; }
        float CurrentRotation { get; }

        bool HasCollisionAtPosition(Vector3 position);

        void SetBuilding(string id);
        void UpdatePosition(Vector3 worldPosition);
        void UpdatePositionFree(Vector3 worldPosition, bool hasGroundSupport);
        void UpdateRotation(float rotation);
        bool Build(Vector3 worldPosition, bool useGrid = true);
        void Cancel();
    }
}