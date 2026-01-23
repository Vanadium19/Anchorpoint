using System.Collections.Generic;
using UnityEngine;
namespace CampBuild
{
    public sealed class GridVisual : MonoBehaviour
    {
        [SerializeField] private int gridSizeX = 10;
        [SerializeField] private int gridSizeZ = 10;
        [SerializeField] private float tileSize = 1f;
        [SerializeField] private float yOffset = 0.02f;
        [SerializeField] private float lineWidth = 0.03f;

        private readonly List<LineRenderer> _lines = new();

        private void Awake()
        {
            BuildLines();
            SetVisible(false);
        }

        public void SetVisible(bool isVisible)
        {
            gameObject.SetActive(isVisible);
        }

        private void BuildLines()
        {
            ClearLines();

 
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


            line.material = new Material(Shader.Find("Sprites/Default"));

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