using System;
using BaseModule;
using InputModule;
using Zenject;

namespace GameCycleModule
{
    public class VictoryPresenter : IInitializable, IDisposable
    {
        private readonly VictoryView _view;
        private readonly IBaseLevelService _baseLevelService;
        private readonly IGamePauseService _pauseService;
        private readonly IInputService _inputService;

        private bool _isShown;

        public VictoryPresenter(VictoryView view,
            IBaseLevelService baseLevelService,
            IGamePauseService pauseService,
            IInputService inputService)
        {
            _view = view;
            _baseLevelService = baseLevelService;
            _pauseService = pauseService;
            _inputService = inputService;
        }

        public void Initialize()
        {
            _view.Hide();
            _baseLevelService.LevelChanged += OnLevelChanged;
            ShowIfTargetReached();
        }

        public void Dispose()
        {
            _baseLevelService.LevelChanged -= OnLevelChanged;

            if (!_isShown)
                return;

            _pauseService.Resume(GamePauseReason.Victory);

            if (!_pauseService.IsPaused)
                _inputService.SetUIMode(false);
        }

        private void OnLevelChanged(int oldLevel, int newLevel) => ShowIfTargetReached();

        private void ShowIfTargetReached()
        {
            if (_isShown || !_baseLevelService.IsTargetLevelReached)
                return;

            _isShown = true;
            _pauseService.Pause(GamePauseReason.Victory);
            _inputService.SetUIMode(true);
            _view.Show();
        }
    }
}