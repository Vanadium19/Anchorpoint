using UnityEngine;

namespace BuildingModule
{
    [CreateAssetMenu(fileName = "GridConfig", menuName = "Game/Configs/Constructing/GridConfig")]
    public class GridConfig : ScriptableObject
    {
        [SerializeField] private int gridSizeX = 10;
        [SerializeField] private int gridSizeZ = 10;
        [SerializeField] private float tileSize = 1f;

        public int GridSizeX => gridSizeX;
        public int GridSizeZ => gridSizeZ;
        public float TileSize => tileSize;
    }
}