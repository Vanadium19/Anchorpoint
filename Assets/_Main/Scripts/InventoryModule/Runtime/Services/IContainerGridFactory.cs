using UnityEngine;

namespace InventoryModule
{
    public interface IContainerGridFactory
    {
        ContainerGridBuildResult BuildContainerGrids(ItemTable itemTable, ContainerMetadata metadata, AbstractGrid fallbackGridPrefab, Transform contentContainer);

        ContainerGridBuildResult BuildMainInventoryGrid(GridTable grid, AbstractGrid gridPrefab, Transform contentContainer);

        ContainerGridBuildResult BuildMainInventoryWithPanel(ContainerGridsData containerPanelPrefab, AbstractGrid fallbackGridPrefab, GridTable existingGrid, Transform contentContainer);
    }
}
