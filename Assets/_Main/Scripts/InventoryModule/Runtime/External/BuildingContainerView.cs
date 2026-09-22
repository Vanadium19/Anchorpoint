using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace InventoryModule
{
    public class BuildingContainerView : MonoBehaviour
    {
        [SerializeField] private AbstractGrid gridPrefab;
        [SerializeField] private Transform gridContainer;
        [SerializeField] private List<AbstractGrid> presetGrids = new();

        private readonly List<AbstractGrid> _grids = new();
        private IExternalUI _container;
        private IInventoryManager _inventoryManager;
        private DiContainer _diContainer;
        private bool _isInitialized;
        private bool _isGridViewSubscribed;

        public IExternalUI Container => _container;

        private void OnEnable()
        {
            SubscribeToGridChanges();
            CreateAndBindMissingGrids();
            RegisterGrids();
        }

        private void OnDisable()
        {
            UnsubscribeFromGridChanges();
            UnregisterGrids();
        }

        private void OnDestroy() => UnsubscribeFromGridChanges();

        public void Initialize(IExternalUI container, IInventoryManager inventoryManager, DiContainer diContainer)
        {
            if (_isInitialized && _container == container)
                return;

            UnsubscribeFromGridChanges();
            _container = container;
            _inventoryManager = inventoryManager;
            _diContainer = diContainer;
            _isInitialized = true;

            SubscribeToGridChanges();
            CreateAndBindMissingGrids();
            RegisterGrids();
        }

        private void CreateAndBindMissingGrids()
        {
            if (!(_container is IInventoryGridView gridView))
                return;

            for (var gridIndex = _grids.Count; gridIndex < gridView.Grids.Count; gridIndex++)
            {
                var gridTable = gridView.Grids[gridIndex];
                AbstractGrid grid;

                if (gridIndex < presetGrids.Count && presetGrids[gridIndex] != null)
                {
                    grid = presetGrids[gridIndex];
                }
                else if (gridPrefab != null && _diContainer != null)
                {
                    var parent = gridContainer != null ? gridContainer : transform;
                    grid = _diContainer.InstantiatePrefabForComponent<AbstractGrid>(gridPrefab, parent);
                }
                else
                {
                    continue;
                }

                grid.SetGridTableOnly(gridTable);
                _grids.Add(grid);
            }
        }

        private void RegisterGrids()
        {
            if (_inventoryManager == null || !(_container is IInventoryGridView gridView))
                return;

            for (var index = 0; index < _grids.Count && index < gridView.Grids.Count; index++)
            {
                var grid = _grids[index];
                var gridTable = gridView.Grids[index];

                grid.SetGridTableOnly(gridTable);
                _inventoryManager.RegisterAdditionalGrid(gridTable);
            }
        }

        private void UnregisterGrids()
        {
            if (_inventoryManager == null || !(_container is IInventoryGridView gridView))
                return;

            foreach (var gridTable in gridView.Grids)
                _inventoryManager.UnregisterAdditionalGrid(gridTable);
        }

        private void SubscribeToGridChanges()
        {
            if (_isGridViewSubscribed || !(_container is IInventoryGridView gridView))
                return;

            gridView.GridsChanged += OnGridsChanged;
            _isGridViewSubscribed = true;
        }

        private void UnsubscribeFromGridChanges()
        {
            if (!_isGridViewSubscribed || !(_container is IInventoryGridView gridView))
                return;

            gridView.GridsChanged -= OnGridsChanged;
            _isGridViewSubscribed = false;
        }

        private void OnGridsChanged()
        {
            CreateAndBindMissingGrids();
            RegisterGrids();
        }
    }
}
