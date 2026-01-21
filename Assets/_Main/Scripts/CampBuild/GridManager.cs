using UnityEngine;

public sealed class GridManager : MonoBehaviour
{
    [SerializeField] private int gridSizeX = 10;
    [SerializeField] private int gridSizeZ = 10;
    [SerializeField] private float tileSize = 1f;

    private Tile[,] _tiles;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        _tiles = new Tile[gridSizeX, gridSizeZ];

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                var worldPosition = new Vector3(
                    x * tileSize,
                    0f,
                    z * tileSize
                );

                _tiles[x, z] = new Tile(worldPosition);
            }
        }
    }

    public Tile GetNearestTile(Vector3 worldPosition)
    {
        var x = Mathf.RoundToInt(worldPosition.x / tileSize);
        var z = Mathf.RoundToInt(worldPosition.z / tileSize);

        if (x < 0 || z < 0 || x >= gridSizeX || z >= gridSizeZ)
            return null;

        return _tiles[x, z];
    }
}
