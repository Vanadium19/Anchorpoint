using UnityEngine;

namespace BuildingModule
{
    public class BuildingFactory
    {
        private readonly BuildingCatalog _catalog;

        public BuildingFactory(BuildingCatalog catalog)
        {
            _catalog = catalog;
        }

        public BuildingView Create(BuildingName name, Vector3 position, Quaternion rotation)
        {
            if (!_catalog.TryGetConfig(name, out var config))
                return null;

            var view = Object.Instantiate(config.Prefab, position, rotation);
            view.SetBuildingName(name);
            return view;
        }
    }
}