using UnityEngine;

namespace BuildingModule
{
    public class GridOverlay : MonoBehaviour
    {
        [SerializeField] private GridConfig gridConfig;

        private void OnDrawGizmos()
        {
            if (gridConfig == null)
                return;

            var tileSize = gridConfig.TileSize;
            Gizmos.color = Color.green;

            for (int x = 0; x < gridConfig.GridSizeX; x++)
            {
                for (int z = 0; z < gridConfig.GridSizeZ; z++)
                {
                    var tilePosition = new Vector3(x * tileSize, 0f, z * tileSize);
                    Gizmos.DrawWireCube(tilePosition, new(tileSize, 0f, tileSize));
                }
            }
        }
    }
}