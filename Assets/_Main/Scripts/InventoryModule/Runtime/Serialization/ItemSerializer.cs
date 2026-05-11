using System.Collections.Generic;
using UnityEngine;

namespace InventoryModule
{
    public static class ItemSerializer
    {
        public static ItemMemento Serialize(ItemTable item)
        {
            if (item == null)
                return null;

            var memento = new ItemMemento
            {
                ItemDataName = item.ItemDataSo?.name ?? "",
                X = item.Position?.X ?? 0,
                Y = item.Position?.Y ?? 0,
                StackCount = item.StackCount,
                IsRotated = item.IsRotated
            };

            if (item.HasDurability && item.DurabilityMetadata != null)
                memento.DurabilityCurrent = item.DurabilityMetadata.Current;

            if (item.IsContainer)
            {
                var metadata = item.GetMetadata<ContainerMetadata>();

                if (metadata?.Inventories != null)
                {
                    foreach (var nestedGrid in metadata.Inventories)
                    {
                        var containerMemento = new ContainerMemento();
                        var nestedItems = nestedGrid.GetAllItems();

                        foreach (var nestedItem in nestedItems)
                        {
                            containerMemento.Items.Add(Serialize(nestedItem));
                        }

                        memento.NestedContainers.Add(containerMemento);
                    }
                }
            }

            return memento;
        }

        public static ItemTable Deserialize(ItemMemento memento, ItemDataSo itemData)
        {
            if (memento == null || itemData == null)
                return null;

            var item = new ItemTable(itemData)
            {
                StackCount = memento.StackCount
            };

            if (memento.IsRotated)
                item.Rotate();

            if (item.HasDurability && item.DurabilityMetadata != null && memento.DurabilityCurrent > 0)
                item.DurabilityMetadata.SetCurrent(memento.DurabilityCurrent);

            return item;
        }

        public static void RestoreNestedContainers(ItemTable item, ItemMemento memento, ItemCatalog catalog)
        {
            if (item == null || memento == null || memento.NestedContainers.Count <= 0)
                return;

            if (!item.IsContainer)
                return;

            var metadata = item.GetMetadata<ContainerMetadata>();
            
            if (metadata?.Inventories == null)
                return;

            for (int i = 0; i < memento.NestedContainers.Count && i < metadata.Inventories.Count; i++)
            {
                var nestedGrid = metadata.Inventories[i];
                var containerMemento = memento.NestedContainers[i];

                foreach (var nestedItemMemento in containerMemento.Items)
                {
                    RestoreItem(nestedGrid, nestedItemMemento, catalog);
                }
            }
        }

        public static void RestoreItem(GridTable grid, ItemMemento memento, ItemCatalog catalog)
        {
            if (grid == null || memento == null || catalog == null)
                return;

            var itemData = catalog.GetByName(memento.ItemDataName);

            if (itemData == null)
                return;

            var item = Deserialize(memento, itemData);
            grid.PlaceItem(item, memento.X, memento.Y);

            RestoreNestedContainers(item, memento, catalog);
        }
    }
}
