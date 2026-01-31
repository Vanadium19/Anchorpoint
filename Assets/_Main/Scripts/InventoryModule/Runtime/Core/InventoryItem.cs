using UnityEngine;

public class InventoryItem
{
    public string Id { get; }
    public Vector2Int Position { get; internal set; }
    private readonly int _baseWidth;
    private readonly int _baseHeight;
    public int MaxStack { get; }
    public int Amount { get; set; }
    public bool IsRotated { get; internal set; }

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

    public InventoryItem(string id, Vector2Int position, Vector2Int size, int maxStack, int amount)
    {
        Id = id;
        Position = position;
        _baseWidth = size.x;
        _baseHeight = size.y;
        MaxStack = maxStack;
        Amount = amount;
        IsRotated = false;
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