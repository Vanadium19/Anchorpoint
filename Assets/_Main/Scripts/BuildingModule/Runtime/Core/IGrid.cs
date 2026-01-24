using UnityEngine;

namespace BuildingModule
{
    public interface IGrid
    {
        int RowsCount { get; }
        int ColumnsCount { get; }

        float TileSize { get; }

        bool TryGetNearestTile(Vector3 worldPosition, out Tile tile);
    }
}