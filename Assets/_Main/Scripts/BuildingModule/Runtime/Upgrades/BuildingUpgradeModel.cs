using UnityEngine;

namespace BuildingModule
{
    public class BuildingUpgradeModel
    {
        private readonly BuildingUpgradeConfig _config;

        private int _level = BuildingUpgradeConfig.BaseLevel;

        public BuildingUpgradeModel(BuildingUpgradeConfig config)
        {
            _config = config;
        }

        public int Level => _level;

        public bool CanUpgrade => _config != null && _level < _config.MaxLevel;

        public BuildingUpgradeLevel NextLevel => GetUpgrade(_level + 1);

        public BuildingUpgradeLevel CurrentLevel => GetUpgrade(_level);

        public void Upgrade()
        {
            if (CanUpgrade)
                _level++;
        }

        public void Restore(int level)
        {
            var maxLevel = _config != null ? _config.MaxLevel : BuildingUpgradeConfig.BaseLevel;
            _level = Mathf.Clamp(level, BuildingUpgradeConfig.BaseLevel, maxLevel);
        }

        private BuildingUpgradeLevel GetUpgrade(int level)
        {
            _config.TryGetUpgrade(level, out var upgrade);
            return upgrade;
        }
    }
}
