using UnityEngine;

namespace InventoryModule
{
    public class InventoryItem
    {
        public ItemName Id { get; }
        public Vector2Int Position { get; }
        public Vector2Int Size { get; }
        public int MaxStack { get; }
        public int Amount { get; private set; }

        public InventoryItem(ItemName id, Vector2Int position, Vector2Int size, int maxStack, int amount)
        {
            Id = id;
            Position = position;
            Size = size;
            MaxStack = maxStack;
            Amount = amount;
        }

        public bool TryAddAmount(int value, out int remainder)
        {
            int space = MaxStack - Amount;
            int toAdd = Mathf.Min(space, value);
            Amount += toAdd;
            remainder = value - toAdd;
            return remainder < value;
        }
    }
}