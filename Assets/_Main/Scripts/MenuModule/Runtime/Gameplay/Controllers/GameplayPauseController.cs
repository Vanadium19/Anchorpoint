using System;
using InputModule;
using Zenject;

namespace MenuModule
{
    public class GameplayPauseController : IInitializable, ITickable, IDisposable
    {
        private readonly IInputMap _inputMap;
        private readonly IInputService _inputService;
        private readonly IGamePauseService _pauseService;

        public GameplayPauseController(IInputMap inputMap,
            IInputService inputService,
            IGamePauseService pauseService)
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

            if (_pauseService.HasReason(GamePauseReason.Victory))
                return;

            _pauseService.ToggleUserPause();
            ApplyInputMode();
        }

        public void Dispose()
        {
            _pauseService.PauseStateChanged -= OnPauseStateChanged;
        }

        private void OnPauseStateChanged(bool isPaused) => ApplyInputMode();

        private void ApplyInputMode()
        {
            if (_pauseService.HasReason(GamePauseReason.UserPause))
            {
                _inputService.SetUIMode(true);
                return;
            }

            if (!_pauseService.IsPaused)
                _inputService.SetUIMode(false);
        }
    }
}