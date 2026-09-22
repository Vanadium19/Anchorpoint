using System.Collections.Generic;
using UnityEngine;
using InventoryModule;
using UtilsModule;

namespace BaseModule
{
    public class Chest : MonoBehaviour, IExternalUI, IInventoryGridView, IBuildingUpgradeReceiver
    {
        [SerializeField] private string displayName = "Chest";
        [SerializeField] private GameObject uiPrefab;
        [SerializeField] private int gridWidth = 8;
        [SerializeField] private int gridHeight = 4;
        [SerializeField] private List<ChestUpgradeLevel> upgradeLevels = new();

        [Header("Localization")]
        [SerializeField] private string nameKey = "";

        private readonly List<GridTable> _grids = new();

        private int _currentUpgradeLevel = 1;

        public event System.Action GridsChanged;

        public string DisplayName =>
            string.IsNullOrEmpty(nameKey) ? displayName : LocalizedText.Get(nameKey);
        public GameObject UIPrefab => uiPrefab;
        public IReadOnlyList<GridTable> Grids => _grids;

        private void Awake()
        {
            _grids.Add(new GridTable(gridWidth, gridHeight));
        }

        public void ApplyUpgradeLevel(int level)
        {
            var targetLevel = Mathf.Max(1, level);

            if (targetLevel <= _currentUpgradeLevel)
                return;

            var hasChanged = false;

            for (var currentLevel = _currentUpgradeLevel + 1; currentLevel <= targetLevel; currentLevel++)
            {
                var levelIndex = currentLevel - 2;

                if (levelIndex < 0 || levelIndex >= upgradeLevels.Count)
                    continue;

                var upgradeLevel = upgradeLevels[levelIndex];

                if (upgradeLevel == null)
                    continue;

                foreach (var gridSize in upgradeLevel.GridSizes)
                {
                    if (gridSize.x <= 0 || gridSize.y <= 0)
                        continue;

                    _grids.Add(new GridTable(gridSize.x, gridSize.y));
                    hasChanged = true;
                }
            }

            _currentUpgradeLevel = targetLevel;

            if (hasChanged)
                GridsChanged?.Invoke();
        }
    }
}
