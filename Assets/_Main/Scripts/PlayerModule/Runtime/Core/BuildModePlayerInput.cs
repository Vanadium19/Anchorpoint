using InputModule;
using UnityEngine;

namespace PlayerModule
{
    public class BuildModePlayerInput : IPlayerInput
    {
        private readonly IInputMap _inputMap;

        public BuildModePlayerInput(IInputMap inputMap)
        {
            _inputMap = inputMap;
        }

        public Vector2 MoveDirection => _inputMap.BuildMoveInput;
        public Vector2 LookDirection => _inputMap.BuildLookInput;
        public bool IsJumpPressed => _inputMap.IsBuildJumpPressed;
        public bool IsCrouchPressed => false;
        public float LeanDirection => 0f;
    }
}
