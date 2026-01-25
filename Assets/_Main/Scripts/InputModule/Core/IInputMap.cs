using UnityEngine;

namespace InputModule.Core
{
    public interface IInputMap
    {
        // Читаемые свойства (только get)
        Vector2 MoveInput { get; }
        Vector2 LookInput { get; }
        bool IsJumpPressed { get; }
        bool IsCrouchPressed { get; }
        float LeanInput { get; }

        float WeaponScroll { get; }
        int SelectWeaponIndex { get; }
        bool IsFirePressed { get; }
        bool IsAimPressed { get; } 
        bool IsReloadPressed { get; }
        bool IsHolsterPressed { get; }
        bool IsAimHeld { get; } 
        bool IsAimTriggered { get; } 

        // Методы управления состоянием
        void Enable();
        void Disable();
    }
}