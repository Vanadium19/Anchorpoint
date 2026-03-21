using InputModule;
using UnityEngine;

namespace PlayerModule
{
    public class NormalPlayerInput : IPlayerInput
    {
        private readonly IInputMap _inputMap;

        public NormalPlayerInput(IInputMap inputMap)
        {
            _inputMap = inputMap;
        }

        public Vector2 MoveDirection => _inputMap.MoveInput;
        public Vector2 LookDirection => _inputMap.LookInput;
        public bool IsJumpPressed => _inputMap.IsJumpPressed;
        public bool IsCrouchPressed => _inputMap.IsCrouchPressed;
        public float LeanDirection => _inputMap.LeanInput;
    }
}
