using UnityEngine;
using Zenject;

namespace BuildingModule
{
    public class ModeControllers : ITickable
    {
        private readonly IConstructionModeService _service;

        public ModeControllers(IConstructionModeService service)
        {
            _service = service;
        }

        public void Tick()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
                _service.Toggle();
        }
    }
}