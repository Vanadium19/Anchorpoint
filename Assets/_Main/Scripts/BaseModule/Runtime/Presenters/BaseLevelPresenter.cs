using System;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace BaseModule
{
    public class BaseLevelPresenter : IBaseLevelPresenter, IInitializable, IDisposable
    {
        private readonly IBaseLevelService _baseLevelService;
        private readonly BaseLevelView _view;
        private readonly BaseLevelUIConfig _uiConfig;

        private int _displayedPointsInLevel;
        private int _displayedLevel;

        public BaseLevelPresenter(
            IBaseLevelService baseLevelService,
            BaseLevelView view,
            BaseLevelUIConfig uiConfig)
        {
            _baseLevelService = baseLevelService;
            _view = view;
            _uiConfig = uiConfig;

            _displayedPointsInLevel = _baseLevelService.PointsInCurrentLevel;
            _displayedLevel = _baseLevelService.CurrentLevel;
        }

        public void Initialize()
        {
            _baseLevelService.LevelChanged += OnLevelChanged;
            _baseLevelService.PointsChanged += OnPointsChanged;

            _view.SetPreviewPointsFormat(_uiConfig.PreviewPointsFormat);

            UpdateLevelText(_displayedLevel);
            UpdatePointsText(_displayedPointsInLevel);
            UpdateProgressBar();
        }

        public void Dispose()
        {
            _baseLevelService.LevelChanged -= OnLevelChanged;
            _baseLevelService.PointsChanged -= OnPointsChanged;
        }

        public void Show()
        {
            _view.AnimateShow(_uiConfig.SlideOffset, _uiConfig.SlideAnimationDuration);
            UpdateLevelText(_baseLevelService.CurrentLevel);
        }

        public void Hide()
        {
            _view.AnimateHide(_uiConfig.SlideOffset, _uiConfig.SlideAnimationDuration);
        }

        public void HideImmediately()
        {
            HidePreview();
            _view.Hide();
        }

        public void ShowPreview(int buildingPoints)
        {
            var preview = _baseLevelService.GetPreview(buildingPoints);

            _view.SetPreviewPointsFormat(_uiConfig.PreviewPointsFormat);
            _view.ShowPreviewPoints(buildingPoints);
            _view.ShowPreviewFill();

            if (preview.WillLevelUp)
            {
                var levelsGained = preview.NewLevel - preview.CurrentLevel;
                _view.SetLevelText(string.Format(_uiConfig.LevelUpFormat, preview.NewLevel, levelsGained));
            }
            else
                _view.SetLevelText(string.Format(_uiConfig.LevelFormat, preview.CurrentLevel));

            var totalPreviewPoints = preview.CurrentPointsInLevel + preview.PreviewPoints;
            var previewProgress = preview.PointsToNextLevel > 0
                ? Mathf.Clamp01(totalPreviewPoints / (float)preview.PointsToNextLevel)
                : 0f;
            _view.AnimatePreviewProgress(previewProgress, _uiConfig.PreviewProgressAnimationDuration);

            AnimatePreviewPunch();
        }

        public void HidePreview()
        {
            _view.HidePreviewPoints();
            _view.HidePreviewFill();
            UpdateLevelText(_baseLevelService.CurrentLevel);
        }

        private void OnLevelChanged(int oldLevel, int newLevel)
        {
            UpdateLevelText(newLevel);
            AnimateLevelChange();
        }

        private void AnimateLevelChange()
        {
            _view.AnimateLevelPunch(
                _uiConfig.LevelPunchScale,
                _uiConfig.LevelChangeAnimationDuration,
                _uiConfig.LevelPunchVibrato,
                _uiConfig.LevelPunchElasticity);

            _displayedLevel = _baseLevelService.CurrentLevel;
        }

        private void AnimatePreviewPunch()
        {
            _view.AnimatePreviewPunch(
                _uiConfig.LevelPunchScale,
                _uiConfig.LevelChangeAnimationDuration,
                _uiConfig.LevelPunchVibrato,
                _uiConfig.LevelPunchElasticity);
        }

        private void OnPointsChanged(int oldTotalPoints, int newTotalPoints, int oldPointsInLevel, int newPointsInLevel)
        {
            AnimatePointsCountUp(oldPointsInLevel, newPointsInLevel);
        }

        private void UpdateLevelText(int level)
        {
            _view.SetLevelText(string.Format(_uiConfig.LevelFormat, level));
        }

        private void UpdatePointsText(int points)
        {
            var threshold = _baseLevelService.PointsToNextLevel;
            
            if (threshold > 0)
                _view.SetPointsText(string.Format(_uiConfig.PointsFormat, points, threshold));
            else
                _view.SetPointsText(string.Format(_uiConfig.PointsNoThresholdFormat, points));
        }

        private void UpdateProgressBar()
        {
            var pointsInLevel = _baseLevelService.PointsInCurrentLevel;
            var threshold = _baseLevelService.PointsToNextLevel;
            var fill = threshold > 0 ? pointsInLevel / (float)threshold : 1f;
            _view.AnimateProgress(fill, _uiConfig.ProgressAnimationDuration);
        }

        private void AnimatePointsCountUp(int from, int to)
        {
            var tweener = DOTween.To(() => _displayedPointsInLevel, x =>
            {
                _displayedPointsInLevel = Mathf.RoundToInt(x);
                UpdatePointsText(_displayedPointsInLevel);
                UpdateProgressBar();
            }, to, _uiConfig.PointsCountUpDuration);

            tweener.SetEase(Ease.OutQuad);
        }
    }
}
