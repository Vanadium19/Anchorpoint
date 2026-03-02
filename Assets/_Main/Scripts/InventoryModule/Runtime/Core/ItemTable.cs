using System;
using UnityEngine;

namespace InventoryModule
{
    [Serializable]
    public class ItemTable
    {
        public ItemDataSo ItemDataSo { get; }
        public bool IsRotated { get; private set; }
        public Position Position { get; private set; }
        public GridTable CurrentGrid { get; private set; }
        public int StackCount { get; set; } = 1;
        public int Amount { get => StackCount; set => StackCount = value; }
        public string Id => ItemDataSo?.DisplayName?.ToLower().Replace(" ", "_") ?? string.Empty;

        public InventoryMetadata InventoryMetadata { get; }

        // TODO: Код взят из ассета
        public int Width => IsRotated ? ItemDataSo.Height : ItemDataSo.Width;

        public int Height => IsRotated ? ItemDataSo.Width : ItemDataSo.Height;

        public bool CanRotate => ItemDataSo.CanRotate;
        public bool IsStackable => ItemDataSo.IsStackable;
        public int MaxStack => ItemDataSo.MaxStackSize;
        public bool IsContainer => ItemDataSo.IsContainer;

        public event Action UIUpdated;

        public ItemTable(ItemDataSo itemDataSo)
        {
            ItemDataSo = itemDataSo;
            StackCount = 1;

            if (IsContainer)
            {
                InventoryMetadata = new ContainerMetadata();
                InventoryMetadata.Initialize(this);
            }
        }

        public int PlacedWidth { get; private set; }
        public int PlacedHeight { get; private set; }

        // TODO: Код взят из ассета
        public void SetGridProps(GridTable grid, Position position)
        {
            CurrentGrid = grid;
            Position = position;
            PlacedWidth = Width;
            PlacedHeight = Height;
            UIUpdated?.Invoke();
        }

        // TODO: Код взят из ассета
        public void RemoveItselfFromLocation()
        {
            if (CurrentGrid != null)
            {
                CurrentGrid.RemoveItem(this);
                CurrentGrid = null;
            }
        }

        // TODO: Код взят из ассета
        public void Rotate()
        {
            if (CanRotate)
            {
                IsRotated = !IsRotated;

                if (CurrentGrid != null)
                {
                    PlacedWidth = Width;
                    PlacedHeight = Height;
                }

                UIUpdated?.Invoke();
            }
        }

        public bool CanStackWith(ItemTable other)
        {
            if (!IsStackable || !other.IsStackable) 
                return false;

            if (ItemDataSo != other.ItemDataSo) 
                return false;

            if (StackCount >= MaxStack) 
                return false;

            return true;
        }

        public int TryAddToStack(int amount)
        {
            if (!IsStackable) 
                return amount;

            int spaceAvailable = MaxStack - StackCount;
            int toAdd = Mathf.Min(amount, spaceAvailable);
            StackCount += toAdd;
            UIUpdated?.Invoke();

            return amount - toAdd;
        }

        public void AddAmount(int amount)
        {
            StackCount = Mathf.Clamp(StackCount + amount, 0, MaxStack);
            UIUpdated?.Invoke();
        }

        // TODO: Код взят из ассета
        public T GetMetadata<T>() where T : InventoryMetadata
        {
            return InventoryMetadata as T;
        }

        public GridTable ContainerGrid
        {
            get
            {
                var metadata = GetMetadata<ContainerMetadata>();
                return metadata?.Inventories?.Count > 0 ? metadata.Inventories[0] : null;
            }
        }

        public override string ToString()
        {
            return $"{ItemDataSo.DisplayName} ({Width}x{Height})";
        }
    }
}
