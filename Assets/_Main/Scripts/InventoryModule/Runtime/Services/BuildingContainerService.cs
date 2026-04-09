using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace InventoryModule
{
    public class BuildingContainerService : IBuildingContainerService, IInitializable
    {
        private readonly Dictionary<IContainerUI, BuildingContainerView> _activeContainers = new();
        private readonly Dictionary<IContainerUI, BuildingContainerView> _cachedContainers = new();
        private readonly IInventoryManager _inventoryManager;
        private readonly IContainerWindowService _windowService;
        private readonly DiContainer _diContainer;
        private readonly Canvas _canvas;
        private readonly GameObject _externalPanel;

        public BuildingContainerService(
            IInventoryManager inventoryManager,
            IContainerWindowService windowService,
            DiContainer diContainer,
            Canvas canvas,
            GameObject externalPanel)
        {
            _inventoryManager = inventoryManager;
            _windowService = windowService;
            _diContainer = diContainer;
            _canvas = canvas;
            _externalPanel = externalPanel;
        }

        public void Initialize()
        {
            _inventoryManager.InventoryOpened += OnInventoryOpened;
            _inventoryManager.InventoryClosed += OnInventoryClosed;
        }

        private void OnInventoryOpened()
        {
            if (_activeContainers.Count == 0 && _externalPanel != null)
                _externalPanel.SetActive(true);
        }

        private void OnInventoryClosed()
        {
            CloseAllContainers();
        }

        public bool IsContainerOpen(IContainerUI container)
        {
            return _activeContainers.ContainsKey(container);
        }

        public void OpenContainer(IContainerUI container)
        {
            if (container == null || container.UIPrefab == null)
                return;

            if (_activeContainers.ContainsKey(container))
                return;

            if (_externalPanel != null)
                _externalPanel.SetActive(false);

            BuildingContainerView view;

            if (_cachedContainers.TryGetValue(container, out var cachedView))
            {
                view = cachedView;
                view.gameObject.SetActive(true);
            }
            else
            {
                var instance = _diContainer.InstantiatePrefab(container.UIPrefab, _canvas.transform);
                view = instance.GetComponent<BuildingContainerView>();

                if (view == null)
                {
                    Object.Destroy(instance);
                    return;
                }

                view.Initialize(container, _inventoryManager, _diContainer);
                _cachedContainers[container] = view;
            }

            _activeContainers[container] = view;
        }

        public void CloseContainer(IContainerUI container)
        {
            if (!_activeContainers.TryGetValue(container, out var view))
                return;

            CloseNestedWindows(container);

            view.gameObject.SetActive(false);
            _activeContainers.Remove(container);

            if (_inventoryManager.IsInventoryOpen && _activeContainers.Count == 0 && _externalPanel != null)
                _externalPanel.SetActive(true);
        }

        public void CloseAllContainers()
        {
            foreach (var kvp in _activeContainers)
            {
                CloseNestedWindows(kvp.Key);
                kvp.Value.gameObject.SetActive(false);
            }

            _activeContainers.Clear();
        }

        private void CloseNestedWindows(IContainerUI container)
        {
            if (container == null || container.Grids == null)
                return;

            foreach (var grid in container.Grids)
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
