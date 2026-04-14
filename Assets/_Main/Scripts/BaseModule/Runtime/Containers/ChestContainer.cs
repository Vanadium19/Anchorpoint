using System.Collections.Generic;
using UnityEngine;
using InventoryModule;

namespace BaseModule
{
    public class ChestContainer : MonoBehaviour, IContainerUI
    {
        [SerializeField] private string displayName = "Chest";
        [SerializeField] private GameObject uiPrefab;
        [SerializeField] private int gridWidth = 8;
        [SerializeField] private int gridHeight = 4;
        [SerializeField] private Collider interactionCollider;

        private readonly List<GridTable> _grids = new();

        private GridTable _grid;

        public string DisplayName => displayName;
        public GameObject UIPrefab => uiPrefab;
        public IReadOnlyList<GridTable> Grids => _grids;
        public Collider InteractionCollider => interactionCollider;

        private void Awake()
        {
            _grid = new GridTable(gridWidth, gridHeight);
            _grids.Add(_grid);
        }
    }
}
