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
        private IContainerUI _container;
        private IInventoryManager _inventoryManager;
        private DiContainer _diContainer;
        private bool _isInitialized;

        public IContainerUI Container => _container;

        public void Initialize(IContainerUI container, IInventoryManager inventoryManager, DiContainer diContainer)
        {
            if (_isInitialized && _container == container)
                return;

            _container = container;
            _inventoryManager = inventoryManager;
            _diContainer = diContainer;
            _isInitialized = true;

            CreateAndBindGrids();
        }

        private void CreateAndBindGrids()
        {
            if (_grids.Count > 0)
                return;

            var gridIndex = 0;

            foreach (var gridTable in _container.Grids)
            {
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
                    gridIndex++;
                    continue;
                }

                grid.SetGridTableOnly(gridTable);
                _inventoryManager.RegisterAdditionalGrid(gridTable);
                _grids.Add(grid);
                gridIndex++;
            }
        }

        private void OnEnable()
        {
            if (_container == null || _inventoryManager == null || _grids.Count == 0)
                return;

            for (int i = 0; i < _grids.Count && i < _container.Grids.Count; i++)
            {
                var gridTable = _container.Grids[i];
                var grid = _grids[i];

                grid.SetGridTableOnly(gridTable);
                _inventoryManager.RegisterAdditionalGrid(gridTable);
            }
        }

        private void OnDisable()
        {
            if (_container == null || _inventoryManager == null)
                return;

            foreach (var gridTable in _container.Grids)
            {
                _inventoryManager.UnregisterAdditionalGrid(gridTable);
            }
        }
    }
}
