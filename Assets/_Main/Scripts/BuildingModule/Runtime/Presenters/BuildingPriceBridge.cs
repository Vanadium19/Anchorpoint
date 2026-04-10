using System;
using Zenject;

namespace BuildingModule
{
    public class BuildingPriceBridge : IInitializable, IDisposable
    {
        private readonly IBuildingPricePresenter _presenter;
        private readonly IPlacementService _placementService;
        private readonly IStorageService _storageService;
        private readonly IConstructionModeService _constructionModeService;
        private readonly IPreviewService _previewService;

        private string _currentBuildingId;

        public BuildingPriceBridge(
            IBuildingPricePresenter presenter,
            IPlacementService placementService,
            IStorageService storageService,
            IConstructionModeService constructionModeService,
            IPreviewService previewService)
        {
            _presenter = presenter;
            _placementService = placementService;
            _storageService = storageService;
            _constructionModeService = constructionModeService;
            _previewService = previewService;
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
            if (!isActive)
                _presenter.Hide();
        }

        private void OnBuildingChanged(string buildingId)
        {
            _currentBuildingId = buildingId;
            UpdatePrice(buildingId);
        }

        private void OnSelectionCleared()
        {
            _currentBuildingId = null;
            _presenter.Hide();
        }

        private void OnPlacementCompleted()
        {
            UpdatePrice(_currentBuildingId);
        }

        private void UpdatePrice(string buildingId)
        {
            if (string.IsNullOrEmpty(buildingId))
            {
                _presenter.Hide();
                _previewService.SetCanAfford(true);
                return;
            }

            var info = _storageService.GetPriceInfo(buildingId);

            if (info == null || info.Items.Count == 0)
            {
                _presenter.Hide();
                _previewService.SetCanAfford(true);
                return;
            }

            _previewService.SetCanAfford(info.AvailableCount > 0);
            _presenter.Show(info);
        }
    }
}
