using UnityEngine;

public sealed class Tile
{
    public Vector3 WorldPosition { get; }
    public bool IsOccupied { get; private set; }

    public Tile(Vector3 worldPosition)
    {
        WorldPosition = worldPosition;
    }

    public void Occupy() => IsOccupied = true;
}