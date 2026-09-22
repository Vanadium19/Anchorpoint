using System.Collections.Generic;
using System;

namespace BuildingModule
{
    public class BuildingRegistry : IBuildingRegistry
    {
        private readonly List<BuildingView> _buildings = new();

        public IReadOnlyList<BuildingView> Buildings => _buildings;

        public event Action<BuildingView> BuildingRegistered;
        public event Action<BuildingView> BuildingUnregistered;

        public void RegisterBuilding(BuildingView building)
        {
            if (building == null || _buildings.Contains(building))
                return;

            _buildings.Add(building);
            BuildingRegistered?.Invoke(building);
        }

        public void UnregisterBuilding(BuildingView building)
        {
            if (building == null)
                return;

            if (!_buildings.Remove(building))
                return;

            BuildingUnregistered?.Invoke(building);
        }

        public void Clear()
        {
            _buildings.Clear();
        }
    }
}
