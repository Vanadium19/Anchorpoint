using System;
using BaseModule;
using BuildingModule;
using InputModule;
using Zenject;

namespace MenuModule
{
    public class VictoryPresenter : IInitializable, IDisposable
    {
        private readonly VictoryView _view;
        private readonly IBaseLevelService _baseLevelService;
        private readonly IPauseManager _pauseService;
        private readonly IInputService _inputService;
        private readonly IConstructionModeService _constructionModeService;
        private readonly IBaseLevelPresenter _baseLevelPresenter;

        private bool _isShown;

        public VictoryPresenter(VictoryView view,
            IBaseLevelService baseLevelService,
            IPauseManager pauseService,
            IInputService inputService,
            IConstructionModeService constructionModeService,
            IBaseLevelPresenter baseLevelPresenter = null)
        {
            _view = view;
            _baseLevelService = baseLevelService;
            _pauseService = pauseService;
            _inputService = inputService;
            _constructionModeService = constructionModeService;
            _baseLevelPresenter = baseLevelPresenter;
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
        }

        private void OnLevelChanged(int oldLevel, int newLevel) => ShowIfTargetReached();

        private void ShowIfTargetReached()
        {
            if (_isShown || !_baseLevelService.IsTargetLevelReached)
                return;

            _isShown = true;
            _constructionModeService?.SetActive(false);
            _baseLevelPresenter?.HideImmediately();
            _pauseService.Pause(PauseReason.Victory);
            _inputService.SetUIMode(true);
            _view.Show();
        }
    }
}