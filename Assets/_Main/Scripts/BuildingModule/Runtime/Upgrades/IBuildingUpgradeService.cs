using System;

namespace BuildingModule
{
    public interface IBuildingUpgradeService
    {
        event Action<BuildingView> BuildingUpgraded;

        int GetLevel(BuildingView building);
        bool TryGetInfo(BuildingView building, out BuildingUpgradeInfo info);
        bool TryUpgrade(BuildingView building);
        void RestoreLevel(BuildingView building, int level);
    }
}
