using System;
using BaseModule;
using UnityEngine;
using Zenject;
using InputModule;

namespace BuildingModule
{
    public class ModeControllers : IInitializable, ITickable, IDisposable, IPausable
    {
        private readonly IConstructionModeService _service;
        private readonly IInputMap _input;
        private readonly IPauseManager _pauseManager;

        private bool _isPaused;

        public ModeControllers(IConstructionModeService service, IInputMap input, IPauseManager pauseManager)
        {
            _service = service;
            _input = input;
            _pauseManager = pauseManager;
        }

        public void Initialize() => _pauseManager.Register(this);

        public void Dispose() => _pauseManager.Unregister(this);

        public void SetPaused(bool isPaused) => _isPaused = isPaused;

        public void Tick()
        {
            if (_isPaused)
                return;

            if (_input.IsBuildPressed)
                _service.Toggle();
        }
    }
}