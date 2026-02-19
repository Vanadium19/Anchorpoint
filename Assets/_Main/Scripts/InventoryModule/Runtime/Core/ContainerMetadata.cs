using System;
using System.Collections.Generic;
using System.Linq;

namespace InventoryModule
{
    [Serializable]
    public class ContainerMetadata : InventoryMetadata
    {
        public List<GridTable> Inventories { get; private set; } = new List<GridTable>();

        public override void Awake()
        {
            base.Awake();
            SetProps();
        }

        private void SetProps()
        {
            if (Inventories == null)
            {
                Inventories = new List<GridTable>();
            }

            if (Inventories.Count == 0 && ItemTable?.ItemDataSo != null && ItemTable.ItemDataSo.IsContainer)
            {
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
                            {
                                Inventories.Add(new GridTable(grid.GridWidth, grid.GridHeight));
                            }
                        }
                    }
                }

                if (Inventories.Count == 0)
                {
                    Inventories.Add(new GridTable(5, 5));
                }
            }
        }

        public void InitializeInventories()
        {
            SetProps();
        }

        public bool IsInsertingInsideYourself(GridTable grid, HashSet<object> visited = null)
        {
            if (Inventories.Count == 0)
                return false;

            if (visited == null)
                visited = new HashSet<object>();

            if (visited.Contains(this))
                return false;

            visited.Add(this);

            if (Inventories.Contains(grid))
                return true;

            foreach (GridTable gridTable in Inventories)
            {
                ItemTable[] containers = gridTable.GetAllContainers();
                foreach (ItemTable container in containers)
                {
                    if (container?.InventoryMetadata is ContainerMetadata nestedMetadata)
                    {
                        if (nestedMetadata.Inventories.Contains(grid))
                            return true;
                        if (nestedMetadata.IsInsertingInsideYourself(grid, visited))
                            return true;
                    }
                }
            }

            return false;
        }

        public GridResponse PlaceItemInInventory(ItemTable item)
        {
            if (Inventories.Count == 0)
                return GridResponse.InventoryFull;

            foreach (GridTable gridTable in Inventories)
            {
                var pos = gridTable.FindSpaceForObjectAnyDirection(item);
                if (pos != null)
                {
                    if (gridTable == item.CurrentGrid)
                        return GridResponse.AlreadyInserted;

                    GridResponse response = gridTable.PlaceItem(item, pos.Value.x, pos.Value.y);
                    if (response == GridResponse.Inserted)
                    {
                        return GridResponse.Inserted;
                    }
                }
            }

            return GridResponse.InventoryFull;
        }

        public bool ContainsSpaceForItem(ItemTable item)
        {
            foreach (GridTable gridTable in Inventories)
            {
                if (gridTable.FindSpaceForObjectAnyDirection(item) != null)
                    return true;
            }
            return false;
        }

        public List<ItemTable> GetAllItems(bool recursive = false)
        {
            if (recursive)
            {
                List<ItemTable> items = new List<ItemTable>();
                AddAllItemsRecursive(items, Inventories);
                return items;
            }

            return Inventories.SelectMany(g => g.GetAllItems()).ToList();
        }

        private void AddAllItemsRecursive(List<ItemTable> items, List<GridTable> grids)
        {
            foreach (GridTable grid in grids)
            {
                items.AddRange(grid.GetAllItems());

                foreach (ItemTable item in grid.GetAllContainers())
                {
                    if (item?.InventoryMetadata is ContainerMetadata nestedMetadata)
                    {
                        nestedMetadata.AddAllItemsRecursive(items, nestedMetadata.Inventories);
                    }
                }
            }
        }

        public override void Initialize(ItemTable itemTable)
        {
            base.Initialize(itemTable);
            SetProps();
        }
    }
}
