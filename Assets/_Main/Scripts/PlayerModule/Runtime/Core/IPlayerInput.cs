using UnityEngine;

namespace PlayerModule
{
    public interface IPlayerInput
    {
        Vector2 MoveDirection { get; }
        Vector2 LookDirection { get; }
        bool IsJumpPressed { get; }
        bool IsCrouchPressed { get; }
        float LeanDirection { get; }
    }
}
