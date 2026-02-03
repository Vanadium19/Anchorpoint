using UnityEngine;

namespace InventoryModule
{
    public sealed class InventoryItem
    {
        private readonly int _baseWidth;
        private readonly int _baseHeight;

        private int _amount;

        public InventoryItem(string id, Vector2Int position, Vector2Int size, int maxStack, int amount)
        {
            Id = id;
            Position = position;
            _baseWidth = size.x;
            _baseHeight = size.y;
            MaxStack = maxStack;
            _amount = amount;
            IsRotated = false;
        }

        public string Id { get; }
        public Vector2Int Position { get; private set; }
        public bool IsRotated { get; private set; }
        public int MaxStack { get; }
        public int Amount => _amount;

        public Vector2Int Size => IsRotated
            ? new Vector2Int(_baseHeight, _baseWidth)
            : new Vector2Int(_baseWidth, _baseHeight);

        public Vector2Int BaseSize => new Vector2Int(_baseWidth, _baseHeight);

        public Vector2Int GetSize(bool rotated)
        {
            return rotated
                ? new Vector2Int(_baseHeight, _baseWidth)
                : new Vector2Int(_baseWidth, _baseHeight);
        }

        public bool TryAddAmount(int value, out int remainder)
        {
            int space = MaxStack - _amount;
            int toAdd = Mathf.Min(space, value);
            _amount += toAdd;
            remainder = value - toAdd;
            return remainder < value;
        }

        public void SetPosition(Vector2Int position)
        {
            Position = position;
        }

        public void SetAmount(int amount)
        {
            _amount = amount;
        }

        public void AddAmount(int delta)
        {
            _amount += delta;
        }

        public void SetIsRotated(bool isRotated)
        {
            IsRotated = isRotated;
        }
    }
}
