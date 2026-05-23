using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace InventoryModule
{
    public class BuildingContainerHandler : ExternalUIHandler
    {
        private readonly IInventoryManager _inventoryManager;
        private readonly IContainerWindowService _windowService;
        private readonly Dictionary<IExternalUI, BuildingContainerView> _activeViews = new();

        public BuildingContainerHandler(
            IInventoryManager inventoryManager,
            IContainerWindowService windowService,
            ExternalUIManager manager,
            DiContainer diContainer,
            Canvas canvas)
            : base(manager, diContainer, canvas)
        {
            _inventoryManager = inventoryManager;
            _windowService = windowService;
        }

        protected override bool CanHandle(IExternalUI ui) => ui is IInventoryGridView;

        protected override GameObject CreateView(IExternalUI ui)
        {
            var instance = InstantiateView(ui);
            var view = instance.GetComponent<BuildingContainerView>();

            if (view != null)
            {
                view.Initialize(ui, _inventoryManager, DiContainer);
                _activeViews[ui] = view;
            }

            return instance;
        }

        protected override void OnReactivated(IExternalUI ui, GameObject view)
        {
            var buildingView = view.GetComponent<BuildingContainerView>();

            if (buildingView != null)
            {
                buildingView.Initialize(ui, _inventoryManager, DiContainer);
                _activeViews[ui] = buildingView;
            }
        }

        public override void CloseAll()
        {
            foreach (var kvp in _activeViews)
                CloseNestedWindows(kvp.Key);

            _activeViews.Clear();
            base.CloseAll();
        }

        private void CloseNestedWindows(IExternalUI container)
        {
            if (!(container is IInventoryGridView gridView))
                return;

            foreach (var grid in gridView.Grids)
            {
                var items = grid.GetAllItems();
                foreach (var item in items)
                {
                    if (item.IsContainer)
                        _windowService.CloseAllWindowsForItem(item);
                }
            }
        }
    }
}