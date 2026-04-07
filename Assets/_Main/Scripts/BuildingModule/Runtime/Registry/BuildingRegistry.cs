using System.Collections.Generic;

namespace BuildingModule
{
    public class BuildingRegistry : IBuildingRegistry
    {
        private readonly List<BuildingView> _buildings = new();

        public IReadOnlyList<BuildingView> Buildings => _buildings;

        public void RegisterBuilding(BuildingView building)
        {
            if (building != null && !_buildings.Contains(building))
                _buildings.Add(building);
        }

        public void UnregisterBuilding(BuildingView building)
        {
            if (building != null)
                _buildings.Remove(building);
        }

        public void Clear()
        {
            _buildings.Clear();
        }
    }
}
