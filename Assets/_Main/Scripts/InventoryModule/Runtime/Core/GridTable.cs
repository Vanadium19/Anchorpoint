using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InventoryModule
{
    // TODO: Код взят из ассета
    public enum GridResponse
    {
        Inserted,
        OutOfBounds,
        Overlapping,
        InventoryFull,
        InsertInsideYourself,
        AlreadyInserted
    }

    [Serializable]
    public class GridTable
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public ItemTable[,] Slots { get; private set; }

        public event Action<ItemTable> ItemInserted;
        public event Action<ItemTable> ItemRemoved;

        public GridTable(int width, int height)
        {
            Width = width;
            Height = height;
            Slots = new ItemTable[Width, Height];
        }

        public GridResponse PlaceItem(ItemTable item, int posX, int posY, ItemTable ignoreItem = null)
        {
            if (!BoundaryCheck(posX, posY, item.Width, item.Height))
                return GridResponse.OutOfBounds;

            if (!OverlapCheck(posX, posY, item.Width, item.Height, ignoreItem))
                return GridResponse.Overlapping;

            item.RemoveItselfFromLocation();

            for (int x = 0; x < item.Width; x++)
            {
                for (int y = 0; y < item.Height; y++)
                {
                    Slots[posX + x, posY + y] = item;
                }
            }

            item.SetGridProps(this, new Position(posX, posY));
            ItemInserted?.Invoke(item);

            return GridResponse.Inserted;
        }

        // TODO: Код взят из ассета
        public bool OverlapCheck(int posX, int posY, int itemWidth, int itemHeight, ItemTable ignoreItem = null)
        {
            for (int x = 0; x < itemWidth; x++)
            {
                for (int y = 0; y < itemHeight; y++)
                {
                    int checkX = posX + x;
                    int checkY = posY + y;

                    if (checkX < 0 || checkX >= Width || checkY < 0 || checkY >= Height)
                        return false;

                    ItemTable existingItem = Slots[checkX, checkY];
                    if (existingItem != null && existingItem != ignoreItem)
                        return false;
                }
            }
            return true;
        }

        public bool CanPlaceItemInContainer(ItemTable item, int posX, int posY)
        {
            if (!BoundaryCheck(posX, posY, item.Width, item.Height))
                return false;

            if (!OverlapCheck(posX, posY, item.Width, item.Height, item))
                return false;

            if (item.IsContainer)
            {
                var metadata = item.GetMetadata<ContainerMetadata>();
                if (metadata != null && metadata.IsInsertingInsideYourself(this))
                {
                    return false;
                }
            }

            return true;
        }

        public bool CanPlaceItem(ItemTable item, int posX, int posY)
        {
            if (!BoundaryCheck(posX, posY, item.Width, item.Height))
                return false;
            return OverlapCheck(posX, posY, item.Width, item.Height, item);
        }

        // TODO: Код взят из ассета
        public bool BoundaryCheck(int posX, int posY, int itemWidth, int itemHeight)
        {
            return posX >= 0 && posY >= 0 &&
                   posX + itemWidth <= Width &&
                   posY + itemHeight <= Height;
        }

        // TODO: Код взят из ассета
        public ItemTable GetItem(int x, int y)
        {
            try
            {
                return Slots[x, y];
            }
            catch (IndexOutOfRangeException)
            {
                return null;
            }
        }

        public void RemoveItem(ItemTable item)
        {
            if (item == null) return;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (Slots[x, y] == item)
                    {
                        Slots[x, y] = null;
                    }
                }
            }

            item.SetGridProps(null, null);
            ItemRemoved?.Invoke(item);
        }

        public ItemTable PickUpItem(int x, int y)
        {
            ItemTable item = GetItem(x, y);
            if (item == null) return null;

            int width = item.PlacedWidth > 0 ? item.PlacedWidth : item.Width;
            int height = item.PlacedHeight > 0 ? item.PlacedHeight : item.Height;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    int slotX = item.Position.X + i;
                    int slotY = item.Position.Y + j;
                    if (slotX >= 0 && slotX < Width && slotY >= 0 && slotY < Height)
                    {
                        Slots[slotX, slotY] = null;
                    }
                }
            }

            return item;
        }

        // TODO: Код взят из ассета
        public Vector2Int? FindSpaceForObject(ItemTable item)
        {
            for (int y = 0; y <= Height - item.Height; y++)
            {
                for (int x = 0; x <= Width - item.Width; x++)
                {
                    if (OverlapCheck(x, y, item.Width, item.Height, item))
                    {
                        return new Vector2Int(x, y);
                    }
                }
            }
            return null;
        }

        // TODO: Код взят из ассета
        public Vector2Int? FindSpaceForObjectAnyDirection(ItemTable item)
        {
            var result = FindSpaceForObject(item);
            if (result != null) return result;

            if (item.CanRotate && item.ItemDataSo.CanRotate)
            {
                item.Rotate();
                result = FindSpaceForObject(item);
                if (result != null) return result;
                item.Rotate();
            }

            return null;
        }

        // TODO: Код взят из ассета
        public ItemTable[] GetAllItems()
        {
            HashSet<ItemTable> items = new HashSet<ItemTable>();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (Slots[x, y] != null)
                        items.Add(Slots[x, y]);
                }
            }

            return items.ToArray();
        }

        // TODO: Код взят из ассета
        public ItemTable[] GetAllContainers()
        {
            List<ItemTable> containers = new List<ItemTable>();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (Slots[x, y] != null && Slots[x, y].IsContainer)
                        containers.Add(Slots[x, y]);
                }
            }

            return containers.ToArray();
        }
    }
}
