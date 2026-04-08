using UnityEngine;

namespace InputModule
{
    public interface IInputMap
    {
        bool IsBuildMode { get; }
        Vector2 MoveInput { get; }
        Vector2 LookInput { get; }
        bool IsJumpPressed { get; }
        bool IsCrouchPressed { get; }
        float LeanInput { get; }
        bool IsInteractPressed { get; }

        float WeaponScroll { get; }
        int SelectWeaponIndex { get; }
        bool IsFirePressed { get; }
        bool IsFireHeld { get; }
        bool IsAimPressed { get; }
        bool IsReloadPressed { get; }
        bool IsHolsterPressed { get; }
        bool IsAimHeld { get; }
        bool IsAimTriggered { get; }

        bool IsInventoryPressed { get; }
        bool IsRotatePressed { get; }
        bool IsSplitPressed { get; }

        bool IsBuildPressed { get; }

        Vector2 BuildMoveInput { get; }
        Vector2 BuildLookInput { get; }
        bool IsBuildJumpPressed { get; }
        bool IsBuildPlacePressed { get; }
        bool IsBuildCancelPressed { get; }
        bool IsBuildRotateLeftPressed { get; }
        bool IsBuildRotateRightPressed { get; }
        bool IsBuildSelectLeftPressed { get; }
        bool IsBuildSelectRightPressed { get; }
        bool IsBuildCategoryUpPressed { get; }
        bool IsBuildCategoryDownPressed { get; }
        float BuildScroll { get; }
    }
}