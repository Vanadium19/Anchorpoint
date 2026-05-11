using System;
using UnityEngine;

namespace InventoryModule
{
    [Serializable]
    public class ItemTable
    {
        [NonSerialized]
        public GridTable CurrentGrid;

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

            if (HasDurability)
            {
                DurabilityMetadata = new DurabilityMetadata();
                DurabilityMetadata.Initialize(this);
            }
        }

        public ItemDataSo ItemDataSo { get; }
        public bool IsRotated { get; private set; }
        public Position Position { get; private set; }
        public int StackCount { get; set; } = 1;

        public InventoryMetadata InventoryMetadata { get; private set; }
        public DurabilityMetadata DurabilityMetadata { get; private set; }

        public int Width => IsRotated ? ItemDataSo.Height : ItemDataSo.Width;

        public int Height => IsRotated ? ItemDataSo.Width : ItemDataSo.Height;

        public bool CanRotate => ItemDataSo.CanRotate;
        public bool IsStackable => ItemDataSo.IsStackable;
        public int MaxStack => ItemDataSo.MaxStackSize;
        public bool IsContainer => ItemDataSo.IsContainer;
        public bool HasDurability => ItemDataSo.HasDurability;

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
            if (CurrentGrid == null)
                return;

            CurrentGrid.RemoveItem(this);
            CurrentGrid = null;
        }

        // TODO: Код взят из ассета
        public void Rotate()
        {
            if (!CanRotate)
                return;

            IsRotated = !IsRotated;

            if (CurrentGrid != null)
            {
                PlacedWidth = Width;
                PlacedHeight = Height;
            }

            UIUpdated?.Invoke();
        }

        public bool CanStackWith(ItemTable other)
        {
            if (!IsStackable || !other.IsStackable)
                return false;

            if (ItemDataSo != other.ItemDataSo)
                return false;

            return StackCount < MaxStack;
        }

        public int TryAddToStack(int amount)
        {
            if (!IsStackable)
                return amount;

            var spaceAvailable = MaxStack - StackCount;
            var toAdd = Mathf.Min(amount, spaceAvailable);
            StackCount += toAdd;
            UIUpdated?.Invoke();

            return amount - toAdd;
        }

        public void AddAmount(int amount)
        {
            StackCount = Mathf.Clamp(StackCount + amount, 0, MaxStack);
            UIUpdated?.Invoke();
        }

        public T GetMetadata<T>() where T : InventoryMetadata
        {
            if (InventoryMetadata is T containerMeta)
                return containerMeta;

            if (DurabilityMetadata is T durabilityMeta)
                return durabilityMeta;

            return null;
        }

        public override string ToString() => $"{ItemDataSo.DisplayName} ({Width}x{Height})";
    }
}