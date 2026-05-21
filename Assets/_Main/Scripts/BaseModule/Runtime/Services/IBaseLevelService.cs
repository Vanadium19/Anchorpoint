using System;

namespace BaseModule
{
    public interface IBaseLevelService
    {
        event Action<int, int> LevelChanged;
        event Action<int, int, int, int> PointsChanged;

        int CurrentLevel { get; }
        int CurrentPoints { get; }
        int PointsInCurrentLevel { get; }
        int PointsToNextLevel { get; }
        bool HasNextLevel { get; }

        int TargetVictoryLevel { get; }
        bool IsTargetLevelReached { get; }

        void AddPoints(int amount);
        void SetPoints(int points);
        BaseLevelPreview GetPreview(int buildingPoints);
    }
}