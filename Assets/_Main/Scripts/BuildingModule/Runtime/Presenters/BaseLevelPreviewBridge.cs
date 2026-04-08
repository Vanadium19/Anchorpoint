using System;
using BaseModule;
using Zenject;

namespace BuildingModule
{
    public class BaseLevelPreviewBridge : IInitializable, IDisposable
    {
        private readonly IBaseLevelPresenter _baseLevelPresenter;
        private readonly IPlacementService _placementService;
        private readonly IConstructionModeService _constructionModeService;
        private readonly BuildingCatalog _catalog;

        private string _currentBuildingId;

        public BaseLevelPreviewBridge(
            IBaseLevelPresenter baseLevelPresenter,
            IPlacementService placementService,
            IConstructionModeService constructionModeService,
            BuildingCatalog catalog)
        {
            _baseLevelPresenter = baseLevelPresenter;
            _placementService = placementService;
            _constructionModeService = constructionModeService;
            _catalog = catalog;
        }

        public void Initialize()
        {
            _placementService.BuildingChanged += OnBuildingChanged;
            _placementService.SelectionCleared += OnSelectionCleared;
            _placementService.PlacementCompleted += OnPlacementCompleted;
            _constructionModeService.ActiveChanged += OnActiveChanged;
        }

        public void Dispose()
        {
            _placementService.BuildingChanged -= OnBuildingChanged;
            _placementService.SelectionCleared -= OnSelectionCleared;
            _placementService.PlacementCompleted -= OnPlacementCompleted;
            _constructionModeService.ActiveChanged -= OnActiveChanged;
        }

        private void OnActiveChanged(bool isActive)
        {
            if (isActive)
            {
                _baseLevelPresenter.Show();
                UpdatePreview(_placementService.CurrentBuildingId);
            }
            else
                _baseLevelPresenter.Hide();
        }

        private void OnBuildingChanged(string buildingId)
        {
            _currentBuildingId = buildingId;
            UpdatePreview(buildingId);
        }

        private void OnSelectionCleared()
        {
            _currentBuildingId = null;
            _baseLevelPresenter.HidePreview();
        }

        private void OnPlacementCompleted()
        {
            UpdatePreview(_currentBuildingId);
        }

        private void UpdatePreview(string buildingId)
        {
            if (string.IsNullOrEmpty(buildingId))
            {
                _baseLevelPresenter.HidePreview();
                return;
            }

            if (!_catalog.TryGetConfig(buildingId, out var config))
            {
                _baseLevelPresenter.HidePreview();
                return;
            }

            _baseLevelPresenter.ShowPreview(config.BasePoints);
        }
    }
}
