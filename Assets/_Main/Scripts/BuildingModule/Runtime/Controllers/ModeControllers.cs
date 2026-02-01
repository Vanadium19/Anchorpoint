using UnityEngine;
using Zenject;
using InputModule;

namespace BuildingModule
{
    public class ModeControllers : ITickable
    {
        private readonly IConstructionModeService _service;
        private readonly IInputMap _input;

        public ModeControllers(IConstructionModeService service, IInputMap input)
        {
            _service = service;
            _input = input;
        }

        public void Tick()
        {
            if (_input.IsBuildPressed)
                _service.Toggle();
        }
    }
}