using System;
using System.Collections.Generic;
using InputModule;
using UnityEngine;
using Zenject;

namespace BuildingModule
{
    public class BuildingMenuPresenter : IInitializable, ITickable, IDisposable
    {
        private readonly IBuildingMenuService _menuService;
        private readonly IConstructionModeService _constructionModeService;
        private readonly IInputMap _inputMap;
        private readonly IPlacementService _placementService;
        private readonly BuildingMenuView _view;
        private readonly BuildingCatalog _catalog;

        private bool _isActive;

        public BuildingMenuPresenter(
            IBuildingMenuService menuService,
            IConstructionModeService constructionModeService,
            IInputMap inputMap,
            IPlacementService placementService,
            BuildingMenuView view,
            BuildingCatalog catalog)
        {
            _menuService = menuService;
            _constructionModeService = constructionModeService;
            _inputMap = inputMap;
            _placementService = placementService;
            _view = view;
            _catalog = catalog;
        }

        public void Initialize()
        {
            _menuService.Initialize(_catalog);
            _constructionModeService.ActiveChanged += OnActiveChanged;
        }

        public void Dispose()
        {
            _constructionModeService.ActiveChanged -= OnActiveChanged;
        }

        public void Tick()
        {
            if (!_isActive)
                return;

            HandleInput();
        }

        private void HandleInput()
        {
            if (_inputMap.IsBuildSelectLeftPressed)
            {
                _menuService.SelectPrevious();
                _view.SelectPrevious();
                UpdateSelectedBuilding();
            }

            if (_inputMap.IsBuildSelectRightPressed)
            {
                _menuService.SelectNext();
                _view.SelectNext();
                UpdateSelectedBuilding();
            }

            if (_inputMap.IsBuildCategoryUpPressed)
            {
                if (_menuService.IsInCategory())
                    return;

                _menuService.EnterCategory();
                _placementService.Cancel();
                _view.SetItemsWithAnimationDown(_menuService.GetCurrentItems(), 0);
                UpdateSelectedBuilding();
            }

            if (_inputMap.IsBuildCategoryDownPressed)
            {
                if (_menuService.IsInCategory())
                {
                    _menuService.ExitCategory();
                    _placementService.Cancel();
                    _view.SetItemsWithAnimationUp(_menuService.GetCurrentItems(), 0);
                    UpdateSelectedBuilding();
                }
            }
        }

        private void UpdateSelectedBuilding()
        {
            var selectedItem = _menuService.GetSelectedItem();
            
            if (selectedItem != null && !selectedItem.IsCategory)
                _placementService.SetBuilding(selectedItem.Id);
            else
                _placementService.Cancel();
        }

        private void OnActiveChanged(bool isActive)
        {
            _isActive = isActive;

            if (_isActive)
            {
                _view.SetActive(true);
                _view.Show();
                UpdateView();
            }
            else
            {
                _view.Hide();
                _placementService.Cancel();
            }
        }

        private void UpdateView()
        {
            var items = _menuService.GetCurrentItems();
            var selectedIndex = 0;

            var selectedItem = _menuService.GetSelectedItem();

            if (selectedItem != null)
                selectedIndex = items.IndexOf(selectedItem);

            _view.SetItems(items, selectedIndex);
            UpdateSelectedBuilding();
        }
    }
}
