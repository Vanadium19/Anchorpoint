using System.Collections.Generic;

namespace BuildingModule
{
    public interface IBuildingRegistry
    {
        IReadOnlyList<BuildingView> Buildings { get; }

        void RegisterBuilding(BuildingView building);
        void UnregisterBuilding(BuildingView building);
        void Clear();
    }
}
