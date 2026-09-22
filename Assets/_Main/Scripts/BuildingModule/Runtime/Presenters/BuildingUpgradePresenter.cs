using System;
using InventoryModule;
using UnityEngine;
using Zenject;

namespace BuildingModule
{
    public class BuildingUpgradePresenter : IInitializable, IDisposable
    {
        private readonly BuildingUpgradePanelView _view;
        private readonly IBuildingUpgradeService _upgradeService;
        private readonly ExternalUIManager _externalUIManager;

        private BuildingView _currentBuilding;

        public BuildingUpgradePresenter(
            BuildingUpgradePanelView view,
            IBuildingUpgradeService upgradeService,
            ExternalUIManager externalUIManager)
        {
            _view = view;
            _upgradeService = upgradeService;
            _externalUIManager = externalUIManager;
        }

        public void Initialize()
        {
            if (_view != null)
            {
                _view.UpgradeRequested += OnUpgradeRequested;
                _view.CloseRequested += OnCloseRequested;
                _view.Hide();
            }

            _externalUIManager.UIOpened += OnUIOpened;
            _externalUIManager.UIClosed += OnUIClosed;

            if (_externalUIManager.Current != null)
                OnUIOpened(_externalUIManager.Current);
        }

        public void Dispose()
        {
            if (_view != null)
            {
                _view.UpgradeRequested -= OnUpgradeRequested;
                _view.CloseRequested -= OnCloseRequested;
            }

            _externalUIManager.UIOpened -= OnUIOpened;
            _externalUIManager.UIClosed -= OnUIClosed;
        }

        private void OnUIOpened(IExternalUI externalUI)
        {
            var component = externalUI as Component;
            var building = component == null ? null : component.GetComponentInParent<BuildingView>();
            Show(building);
        }

        private void OnUIClosed() => Hide();

        private void OnUpgradeRequested()
        {
            if (_currentBuilding == null)
                return;

            _upgradeService.TryUpgrade(_currentBuilding);
            Show(_currentBuilding);
        }

        private void OnCloseRequested() => Hide();

        private void Show(BuildingView building)
        {
            if (_view == null || !_upgradeService.TryGetInfo(building, out var info))
            {
                Hide();
                return;
            }

            _currentBuilding = building;
            _view.Render(info);
            _view.Show();
        }

        private void Hide()
        {
            _currentBuilding = null;

            if (_view != null)
                _view.Hide();
        }
    }
}
