using UnityEngine;

namespace InputModule
{
    public interface IInputMap
    {
        Vector2 MoveInput { get; }
        Vector2 LookInput { get; }
        bool IsJumpPressed { get; }
        bool IsCrouchPressed { get; }
        float LeanInput { get; }
        bool IsInteractPressed { get; }

        float WeaponScroll { get; }
        int SelectWeaponIndex { get; }
        bool IsFirePressed { get; }
        bool IsAimPressed { get; }
        bool IsReloadPressed { get; }
        bool IsHolsterPressed { get; }
        bool IsAimHeld { get; }
        bool IsAimTriggered { get; }

        bool IsInventoryPressed { get; }
        bool IsRotatePressed { get; }
        bool IsSplitPressed { get; }
    }
}