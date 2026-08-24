using System.Collections.Generic;
using UnityEngine;
using InventoryModule;
using UtilsModule;

namespace BaseModule
{
    public class Chest : MonoBehaviour, IExternalUI, IInventoryGridView
    {
        [SerializeField] private string displayName = "Chest";
        [SerializeField] private GameObject uiPrefab;
        [SerializeField] private int gridWidth = 8;
        [SerializeField] private int gridHeight = 4;

        [Header("Localization")]
        [SerializeField] private string nameKey = "";

        private readonly List<GridTable> _grids = new();

        private GridTable _grid;

        public string DisplayName =>
            string.IsNullOrEmpty(nameKey) ? displayName : LocalizedText.Get(nameKey);
        public GameObject UIPrefab => uiPrefab;
        public IReadOnlyList<GridTable> Grids => _grids;

        private void Awake()
        {
            _grid = new GridTable(gridWidth, gridHeight);
            _grids.Add(_grid);
        }
    }
}
