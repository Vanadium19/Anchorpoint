using UnityEngine;
using Zenject;

namespace InventoryModule
{
    public class CharacterInventory : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private InventoryPanel inventoryPanel;
        [SerializeField] private AbstractGrid containerGridPrefab;

        [Header("Container Panel")]
        [SerializeField] private ContainerGridsData containerPanelPrefab;

        private GridTable _mainGrid;
        private ContainerSection _mainSection;
        private DiContainer _diContainer;

        private IInventoryManager _inventoryManager;

        [Inject]
        public void Construct(DiContainer container)
        {
            _diContainer = container;
        }

        public void Initialize(IInventoryManager inventoryManager)
        {
            _inventoryManager = inventoryManager;

            if (_inventoryManager?.MainGrid != null)
                _mainGrid = _inventoryManager.MainGrid;
            else
                CreateMainGrid();

            InitializeUI();
        }

        private void CreateMainGrid()
        {
            if (containerPanelPrefab == null)
                return;

            containerPanelPrefab.InitializeGrids();
            var prefabGrids = containerPanelPrefab.Grids;

            if (prefabGrids == null || prefabGrids.Length == 0)
                return;

            var firstGrid = prefabGrids[0];
            _mainGrid = new(firstGrid.GridWidth, firstGrid.GridHeight);

            _inventoryManager?.SetMainGrid(_mainGrid);
        }

        private void InitializeUI()
        {
            if (inventoryPanel == null || containerGridPrefab == null || _mainGrid == null)
                return;

            inventoryPanel.Initialize();

            _mainSection = _diContainer != null
                ? _diContainer.InstantiatePrefabForComponent<ContainerSection>(inventoryPanel.SectionPrefab, inventoryPanel.SectionsContainer)
                : Instantiate(inventoryPanel.SectionPrefab, inventoryPanel.SectionsContainer);

            if (containerPanelPrefab != null)
                _mainSection.InitializeAsMainInventoryWithPanel("Inventory", containerPanelPrefab, containerGridPrefab, _mainGrid);
            else
                _mainSection.InitializeAsMainInventory("Inventory", _mainGrid, containerGridPrefab);

            inventoryPanel.AddSectionFirst(_mainSection);
        }
    }
}