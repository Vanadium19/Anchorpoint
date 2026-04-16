using System;
using Zenject;
using UnityEngine;

namespace BuildingModule
{
    public class BuildingPricePresenter : IInitializable, IDisposable
    {
        private readonly BuildingPricePanelView _view;
        private readonly IPlacementService _placementService;
        private readonly IStorageService _storageService;
        private readonly IConstructionModeService _constructionModeService;
        private readonly IPreviewService _previewService;

        private string _currentBuildingId;

        public BuildingPricePresenter(
            BuildingPricePanelView view,
            IPlacementService placementService,
            IStorageService storageService,
            IConstructionModeService constructionModeService,
            IPreviewService previewService)
        {
            _view = view;
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
                Hide();
        }

        private void OnBuildingChanged(string buildingId)
        {
            _currentBuildingId = buildingId;
            UpdatePrice(buildingId);
        }

        private void OnSelectionCleared()
        {
            _currentBuildingId = null;
            Hide();
        }

        private void OnPlacementCompleted()
        {
            UpdatePrice(_currentBuildingId);
        }

        private void UpdatePrice(string buildingId)
        {
            if (string.IsNullOrEmpty(buildingId))
            {
                Hide();
                _previewService.SetCanAfford(true);
                return;
            }

            var info = _storageService.GetPriceInfo(buildingId);

            if (info == null || info.Items == null || info.Items.Count == 0)
            {
                Hide();
                _previewService.SetCanAfford(true);
                return;
            }

            _previewService.SetCanAfford(info.AvailableCount > 0);
            Show(info);
        }

        private void Show(BuildPriceInfo info)
        {
            if (_view == null)
                return;

            _view.gameObject.SetActive(true);
            _view.SetData(info);
        }

        private void Hide()
        {
            if (_view == null)
                return;

            _view.gameObject.SetActive(false);
            _view.Clear();
        }
    }
}

