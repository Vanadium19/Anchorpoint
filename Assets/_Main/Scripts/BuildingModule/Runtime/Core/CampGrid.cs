using UnityEngine;

namespace BuildingModule
{
    public class CampGrid : IGrid
    {
        private readonly Tile[,] _tiles;
        private readonly float _tileSize;

        public CampGrid(Tile[,] tiles, float tileSize)
        {
            _tiles = tiles;
            _tileSize = tileSize;
        }

        public int RowsCount => _tiles.GetLength(0);
        public int ColumnsCount => _tiles.GetLength(1);
        public float TileSize => _tileSize;

        public bool TryGetNearestTile(Vector3 worldPosition, out Tile tile)
        {
            tile = null;

            var x = Mathf.RoundToInt(worldPosition.x / _tileSize);
            var z = Mathf.RoundToInt(worldPosition.z / _tileSize);

            if (x < 0 || z < 0 || x >= RowsCount || z >= ColumnsCount)
                return false;

            tile = _tiles[x, z];
            return true;
        }

        public static CampGrid CreateGrid(GridConfig gridConfig)
        {
            var gridSizeX = gridConfig.GridSizeX;
            var gridSizeZ = gridConfig.GridSizeZ;
            var tileSize = gridConfig.TileSize;

            var tiles = new Tile[gridSizeX, gridSizeZ];

            for (int x = 0; x < gridSizeX; x++)
            {
                for (int z = 0; z < gridSizeZ; z++)
                {
                    var worldPosition = new Vector3(x * tileSize, 0f, z * tileSize);
                    tiles[x, z] = new(worldPosition);
                }
            }

            return new(tiles, tileSize);
        }
    }
}