using System;
using System.Collections.Generic;

namespace InventoryModule
{
    [Serializable]
    public class ContainerMetadata : InventoryMetadata
    {
        public List<GridTable> Inventories { get; private set; } = new();

        public override void Awake()
        {
            base.Awake();
            SetProps();
        }

        private void SetProps()
        {
            Inventories ??= new();

            if (Inventories.Count != 0 || ItemTable?.ItemDataSo == null || !ItemTable.ItemDataSo.IsContainer)
                return;

            var containerGrids = ItemTable.ItemDataSo.ContainerGrids;

            if (containerGrids != null)
            {
                containerGrids.InitializeGrids();
                var grids = containerGrids.Grids;

                if (grids != null)
                {
                    foreach (var grid in grids)
                    {
                        if (grid != null)
                            Inventories.Add(new GridTable(grid.GridWidth, grid.GridHeight));
                    }
                }
            }

            if (Inventories.Count == 0)
                Inventories.Add(new(5, 5));
        }

        public void InitializeInventories() => SetProps();

        public bool IsInsertingInsideYourself(GridTable grid, HashSet<object> visited = null)
        {
            if (Inventories.Count == 0)
                return false;

            visited ??= new();

            if (!visited.Add(this))
                return false;

            if (Inventories.Contains(grid))
                return true;

            foreach (var gridTable in Inventories)
            {
                var containers = gridTable.GetAllContainers();

                foreach (var container in containers)
                {
                    if (container?.InventoryMetadata is not ContainerMetadata nestedMetadata)
                        continue;

                    if (nestedMetadata.Inventories.Contains(grid))
                        return true;

                    if (nestedMetadata.IsInsertingInsideYourself(grid, visited))
                        return true;
                }
            }

            return false;
        }

        public GridResponse PlaceItemInInventory(ItemTable item)
        {
            if (Inventories.Count == 0)
                return GridResponse.InventoryFull;

            foreach (var gridTable in Inventories)
            {
                var position = gridTable.FindSpaceForObjectAnyDirection(item);

                if (position == null)
                    continue;

                if (gridTable == item.CurrentGrid)
                    return GridResponse.AlreadyInserted;

                var response = gridTable.PlaceItem(item, position.Value.x, position.Value.y);

                if (response == GridResponse.Inserted)
                    return GridResponse.Inserted;
            }

            return GridResponse.InventoryFull;
        }

        public override void Initialize(ItemTable itemTable)
        {
            base.Initialize(itemTable);
            SetProps();
        }
    }
}