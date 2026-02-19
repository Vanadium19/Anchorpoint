using UnityEngine;

namespace InventoryModule
{
    public class CharacterInventory : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private InventoryPanel inventoryPanel;
        [SerializeField] private AbstractGrid containerGridPrefab;

        [Header("Container Panel")]
        [SerializeField] private ContainerGridsData containerPanelPrefab;

        public GridTable MainGrid { get; private set; }
        public ContainerSection MainSection { get; private set; }

        private InventoryManager _inventoryManager;

        public void Initialize(InventoryManager inventoryManager)
        {
            _inventoryManager = inventoryManager;

            if (_inventoryManager?.MainGrid != null)
            {
                MainGrid = _inventoryManager.MainGrid;
            }
            else
            {
                CreateMainGrid();
            }

            InitializeUI();
        }

        private void CreateMainGrid()
        {
            if (containerPanelPrefab == null) return;

            containerPanelPrefab.InitializeGrids();
            var prefabGrids = containerPanelPrefab.Grids;

            if (prefabGrids == null || prefabGrids.Length == 0) return;

            var firstGrid = prefabGrids[0];
            MainGrid = new GridTable(firstGrid.GridWidth, firstGrid.GridHeight);

            _inventoryManager?.SetMainGrid(MainGrid);
        }

        private void InitializeUI()
        {
            if (inventoryPanel == null || containerGridPrefab == null || MainGrid == null) return;

            inventoryPanel.Initialize();

            MainSection = Instantiate(inventoryPanel.SectionPrefab, inventoryPanel.SectionsContainer);
            inventoryPanel.EnsureSectionHasLayoutElement(MainSection);

            if (containerPanelPrefab != null)
            {
                MainSection.InitializeAsMainInventoryWithPanel("Inventory", containerPanelPrefab, containerGridPrefab, MainGrid);
            }
            else
            {
                MainSection.InitializeAsMainInventory("Inventory", MainGrid, containerGridPrefab);
            }

            inventoryPanel.AddSectionFirst(MainSection);
        }

        public void OpenInventory() => _inventoryManager?.OpenInventory();
        public void CloseInventory() => _inventoryManager?.CloseInventory();
        public void ToggleInventory() => _inventoryManager?.ToggleInventory();

        public bool PickupItem(ItemDataSo itemData, int amount = 1)
        {
            return _inventoryManager?.AddItemToInventory(itemData, amount) ?? false;
        }
    }
}
