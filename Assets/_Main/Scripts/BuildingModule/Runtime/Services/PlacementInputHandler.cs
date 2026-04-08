using InputModule;
using UnityEngine;

namespace BuildingModule
{
    public class PlacementInputHandler : IPlacementInputHandler
    {
        private readonly IInputMap _inputMap;
        private readonly PlacementConfig _config;

        public PlacementInputHandler(IInputMap inputMap, PlacementConfig config)
        {
            _inputMap = inputMap;
            _config = config;
        }

        public float RotationDelta
        {
            get
            {
                var rotateLeft = _inputMap.IsBuildRotateLeftPressed;
                var rotateRight = _inputMap.IsBuildRotateRightPressed;

                if (!rotateLeft && !rotateRight)
                    return 0f;

                var delta = 0f;

                if (rotateLeft)
                    delta -= _config.RotationSpeed * Time.deltaTime;

                if (rotateRight)
                    delta += _config.RotationSpeed * Time.deltaTime;

                return delta;
            }
        }

        public float ScrollDelta
        {
            get
            {
                var scroll = _inputMap.BuildScroll;
                
                if (Mathf.Abs(scroll) < _config.InputTolerance)
                    return 0f;

                return scroll * _config.ScrollSensitivity * Time.deltaTime;
            }
        }

        public bool IsPlacePressed => _inputMap.IsBuildPlacePressed;

        public bool IsCancelPressed => _inputMap.IsBuildCancelPressed;
    }
}
