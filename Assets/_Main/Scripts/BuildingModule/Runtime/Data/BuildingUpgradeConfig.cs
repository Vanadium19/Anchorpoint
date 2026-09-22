using System;
using UnityEngine;

namespace BuildingModule
{
    [CreateAssetMenu(fileName = "BuildingUpgradeConfig", menuName = "Game/Configs/Constructing/BuildingUpgradeConfig")]
    public class BuildingUpgradeConfig : ScriptableObject
    {
        public const int LevelCount = 3;

        [SerializeField] private BuildingUpgradeLevel[] levels = new BuildingUpgradeLevel[LevelCount];

        private void OnValidate()
        {
            if (levels == null)
                levels = new BuildingUpgradeLevel[LevelCount];

            if (levels.Length != LevelCount)
                Array.Resize(ref levels, LevelCount);

            for (var index = 0; index < LevelCount; index++)
            {
                if (levels[index] == null)
                    levels[index] = new BuildingUpgradeLevel();
            }
        }

        public bool TryGetLevel(int level, out BuildingUpgradeLevel upgradeLevel)
        {
            upgradeLevel = null;

            if (level < 1 || level > LevelCount || levels == null || levels.Length != LevelCount)
                return false;

            upgradeLevel = levels[level - 1];
            return upgradeLevel != null;
        }
    }
}
