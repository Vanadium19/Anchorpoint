using BaseModule;
using System;
using InputModule;
using Zenject;

namespace MenuModule
{
    public class GameplayPauseController : IInitializable, ITickable, IDisposable
    {
        private readonly IInputMap _inputMap;
        private readonly IInputService _inputService;
        private readonly IPauseManager _pauseService;

        public GameplayPauseController(IInputMap inputMap,
            IInputService inputService,
            IPauseManager pauseService)
        {
            _inputMap = inputMap;
            _inputService = inputService;
            _pauseService = pauseService;
        }

        public void Initialize()
        {
            _pauseService.PauseStateChanged += OnPauseStateChanged;
            ApplyInputMode();
        }

        public void Tick()
        {
            if (!_inputMap.IsPausePressed)
                return;

            if (_pauseService.HasReason(PauseReason.Victory))
                return;

            _pauseService.ToggleUserPause();
            ApplyInputMode();
        }

        public void Dispose()
        {
            _pauseService.PauseStateChanged -= OnPauseStateChanged;
            _inputService.SetUIMode(false);
            _pauseService.Clear();
        }

        private void OnPauseStateChanged(bool isPaused) => ApplyInputMode();

        private void ApplyInputMode()
        {
            if (_pauseService.HasReason(PauseReason.UserPause))
            {
                _inputService.SetUIMode(true);
                return;
            }

            if (!_pauseService.IsPaused)
                _inputService.SetUIMode(false);
        }
    }
}