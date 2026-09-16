using System;
using ComponentsModule;
using Zenject;

namespace BuildingModule
{
    public class BuildingRepairPresenter : IInitializable, IDisposable
    {
        private readonly IInteractionFocusService _interactionFocusService;
        private readonly BuildingPricePanelView _priceView;
        private readonly BuildingCatalog _buildingCatalog;
        private readonly IStorageService _storageService;
        private readonly IConstructionModeService _constructionModeService;

        private BuildingView _currentBuilding;
        private bool _isConstructionModeActive;

        public BuildingRepairPresenter(
            IInteractionFocusService interactionFocusService,
            BuildingPricePanelView priceView,
            BuildingCatalog buildingCatalog,
            IStorageService storageService,
            IConstructionModeService constructionModeService)
        {
            _interactionFocusService = interactionFocusService;
            _priceView = priceView;
            _buildingCatalog = buildingCatalog;
            _storageService = storageService;
            _constructionModeService = constructionModeService;
        }

        public void Initialize()
        {
            _interactionFocusService.InteractableChanged += OnInteractableHoverChanged;
            _constructionModeService.ActiveChanged += OnConstructionModeActiveChanged;
        }

        public void Dispose()
        {
            _interactionFocusService.InteractableChanged -= OnInteractableHoverChanged;
            _constructionModeService.ActiveChanged -= OnConstructionModeActiveChanged;
        }

        private void OnInteractableHoverChanged(IInteractable interactable)
        {
            if (_isConstructionModeActive)
            {
                _currentBuilding = null;
                return;
            }

            if (interactable is not BuildingView buildingView)
            {
                _currentBuilding = null;
                Hide();
                return;
            }

            if (!buildingView.TryGet<BuildingModel>(out var model) || model.State != BuildingState.Broken)
            {
                _currentBuilding = null;
                Hide();
                return;
            }

            _currentBuilding = buildingView;
            UpdatePrice();
        }

        private void OnConstructionModeActiveChanged(bool isActive)
        {
            _isConstructionModeActive = isActive;

            if (isActive)
            {
                _currentBuilding = null;
                Hide();
            }
        }

        private void UpdatePrice()
        {
            if (_currentBuilding == null)
            {
                Hide();
                return;
            }

            if (!_buildingCatalog.TryGetConfig(_currentBuilding.BuildingConfigId, out var config))
            {
                Hide();
                return;
            }

            var info = _storageService.GetPriceInfo(config.DisplayName, config.RepairPrice);

            if (info == null || info.Items == null || info.Items.Count == 0)
            {
                Hide();
                return;
            }

            if (_priceView == null)
                return;

            _priceView.gameObject.SetActive(true);
            _priceView.SetData(info);
        }

        private void Hide()
        {
            if (_priceView == null)
                return;

            _priceView.gameObject.SetActive(false);
            _priceView.Clear();
        }
    }
}
