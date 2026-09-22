namespace BuildingModule
{
    public class BuildingUpgradeModel
    {
        private readonly BuildingUpgradeConfig _config;

        private int _level = 1;

        public BuildingUpgradeModel(BuildingUpgradeConfig config)
        {
            _config = config;
        }

        public int Level => _level;

        public bool CanUpgrade => _config != null
            && _level < BuildingUpgradeConfig.LevelCount
            && _config.TryGetLevel(_level + 1, out _);

        public BuildingUpgradeLevel NextLevel
        {
            get
            {
                _config.TryGetLevel(_level + 1, out var level);
                return level;
            }
        }

        public BuildingUpgradeLevel CurrentLevel
        {
            get
            {
                _config.TryGetLevel(_level, out var level);
                return level;
            }
        }

        public void Upgrade()
        {
            if (CanUpgrade)
                _level++;
        }

        public void Restore(int level)
        {
            _level = level <= 0 ? 1 : level;
            _level = System.Math.Min(System.Math.Max(_level, 1), BuildingUpgradeConfig.LevelCount);
        }
    }
}
