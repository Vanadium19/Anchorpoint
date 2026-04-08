using UnityEngine;

namespace BuildingModule
{
    public interface IPlacementInputHandler
    {
        float RotationDelta { get; }
        float ScrollDelta { get; }
        bool IsPlacePressed { get; }
        bool IsCancelPressed { get; }
    }
}
