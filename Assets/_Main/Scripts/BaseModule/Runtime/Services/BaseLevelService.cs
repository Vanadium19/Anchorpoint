using System;

namespace BaseModule
{
    public class BaseLevelService : IBaseLevelService
    {
        private readonly int[] _deltaThresholds;
        private readonly int[] _cumulativeThresholds;
        private int _currentPoints;

        public event Action<int, int> LevelChanged;
        public event Action<int, int, int, int> PointsChanged;

        public BaseLevelService(BaseLevelConfig config)
        {
            _deltaThresholds = (int[])config.LevelThresholds.Clone();
            _cumulativeThresholds = CalculateCumulativeThresholds(_deltaThresholds);
        }

        public int CurrentLevel => CalculateLevel(_currentPoints);

        public int CurrentPoints => _currentPoints;

        public int PointsToNextLevel
        {
            get
            {
                var level = CurrentLevel;
                var index = level - 1;

                if (index < 0 || index >= _deltaThresholds.Length)
                    return 0;

                return _deltaThresholds[index];
            }
        }

        public int PointsInCurrentLevel
        {
            get
            {
                var level = CurrentLevel;

                if (level <= 1)
                    return _currentPoints;

                var currentLevelThreshold = GetCumulativeThreshold(level);
                return _currentPoints - currentLevelThreshold;
            }
        }

        public bool HasNextLevel => CurrentLevel - 1 < _deltaThresholds.Length;

        public void AddPoints(int amount)
        {
            var oldLevel = CurrentLevel;
            var oldPointsInLevel = GetPointsInLevel(_currentPoints, oldLevel);
            var oldPoints = _currentPoints;

            _currentPoints += amount;

            var newLevel = CurrentLevel;
            var newPointsInLevel = GetPointsInLevel(_currentPoints, newLevel);
            var newPoints = _currentPoints;

            if (oldLevel != newLevel)
                LevelChanged?.Invoke(oldLevel, newLevel);

            PointsChanged?.Invoke(oldPoints, newPoints, oldPointsInLevel, newPointsInLevel);
        }

        public BaseLevelPreview GetPreview(int buildingPoints)
        {
            var previewPoints = _currentPoints + buildingPoints;
            var currentLevel = CurrentLevel;
            var newLevel = CalculateLevel(previewPoints);

            var currentPointsInLevel = GetPointsInLevel(_currentPoints, currentLevel);
            var newPointsInLevel = GetPointsInLevel(previewPoints, newLevel);

            return new BaseLevelPreview
            {
                CurrentLevel = currentLevel,
                CurrentPoints = _currentPoints,
                CurrentPointsInLevel = currentPointsInLevel,
                PointsToNextLevel = PointsToNextLevel,
                PreviewPoints = buildingPoints,
                NewLevel = newLevel,
                NewPoints = previewPoints,
                NewPointsInLevel = newPointsInLevel,
                NewPointsToNextLevel = GetDeltaForLevel(newLevel),
                WillLevelUp = newLevel > currentLevel
            };
        }

        private int CalculateLevel(int points)
        {
            if (_cumulativeThresholds.Length == 0)
                return 1;

            var level = 1;

            for (var i = 0; i < _cumulativeThresholds.Length; i++)
            {
                if (points >= _cumulativeThresholds[i])
                    level = i + 2;
                else
                    break;
            }

            return level;
        }

        private int GetDeltaForLevel(int level)
        {
            var index = level - 1;

            if (index < 0 || index >= _deltaThresholds.Length)
                return 0;

            return _deltaThresholds[index];
        }

        private int GetCumulativeThreshold(int level)
        {
            if (level <= 1)
                return 0;

            var index = level - 2;

            if (index < 0)
                return 0;

            if (index >= _cumulativeThresholds.Length)
                return _cumulativeThresholds[_cumulativeThresholds.Length - 1];

            return _cumulativeThresholds[index];
        }

        private int GetPointsInLevel(int points, int level)
        {
            if (level <= 1)
                return points;

            var currentThreshold = GetCumulativeThreshold(level);
            return points - currentThreshold;
        }

        private int[] CalculateCumulativeThresholds(int[] deltas)
        {
            if (deltas.Length == 0)
                return Array.Empty<int>();

            var cumulative = new int[deltas.Length];
            cumulative[0] = deltas[0];
            
            for (var i = 1; i < deltas.Length; i++)
                cumulative[i] = cumulative[i - 1] + deltas[i];

            return cumulative;
        }
    }
}
