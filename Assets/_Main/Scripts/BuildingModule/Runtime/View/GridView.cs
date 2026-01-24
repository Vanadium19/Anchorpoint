using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace BuildingModule
{
    public sealed class GridView : MonoBehaviour
    {
        [SerializeField] private float yOffset = 0.02f;
        [SerializeField] private float lineWidth = 0.03f;

        private readonly List<LineRenderer> _lines = new();

        [Inject]
        public void Construct(IGrid grid)
        {
            BuildLines(grid);
        }

        private void BuildLines(IGrid grid)
        {
            ClearLines();

            var gridSizeX = grid.RowsCount;
            var gridSizeZ = grid.ColumnsCount;

            var tileSize = grid.TileSize;

            for (int x = 0; x <= gridSizeX; x++)
            {
                var start = new Vector3(x * tileSize, yOffset, 0f);
                var end = new Vector3(x * tileSize, yOffset, gridSizeZ * tileSize);
                _lines.Add(CreateLine(start, end));
            }

            for (int z = 0; z <= gridSizeZ; z++)
            {
                var start = new Vector3(0f, yOffset, z * tileSize);
                var end = new Vector3(gridSizeX * tileSize, yOffset, z * tileSize);
                _lines.Add(CreateLine(start, end));
            }
        }

        private LineRenderer CreateLine(Vector3 start, Vector3 end)
        {
            var lineObject = new GameObject("GridLine");
            lineObject.transform.SetParent(transform, false);

            var line = lineObject.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            line.widthMultiplier = lineWidth;
            line.material = new(Shader.Find("Sprites/Default"));

            return line;
        }

        private void ClearLines()
        {
            for (int i = 0; i < _lines.Count; i++)
            {
                if (_lines[i] != null)
                    Destroy(_lines[i].gameObject);
            }

            _lines.Clear();
        }
    }
}