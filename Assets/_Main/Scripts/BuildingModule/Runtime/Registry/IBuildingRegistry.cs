using System.Collections.Generic;
using System;

namespace BuildingModule
{
    public interface IBuildingRegistry
    {
        IReadOnlyList<BuildingView> Buildings { get; }

        event Action<BuildingView> BuildingRegistered;
        event Action<BuildingView> BuildingUnregistered;

        void RegisterBuilding(BuildingView building);
        void UnregisterBuilding(BuildingView building);
        void Clear();
    }
}
