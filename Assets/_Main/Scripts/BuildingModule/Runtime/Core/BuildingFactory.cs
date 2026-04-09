using UnityEngine;

namespace BuildingModule
{
    public class BuildingFactory
    {
        private readonly BuildingCatalog _catalog;
        private readonly IBuildingRegistry _registry;

        public BuildingFactory(BuildingCatalog catalog, IBuildingRegistry registry)
        {
            _catalog = catalog;
            _registry = registry;
        }

        public BuildingView Create(string id, Vector3 position, Quaternion rotation)
        {
            if (!_catalog.TryGetConfig(id, out var config))
                return null;

            var view = Object.Instantiate(config.Prefab, position, rotation);

            if (view.CollisionCollider != null)
                view.CollisionCollider.enabled = false;

            _registry.RegisterBuilding(view);

            return view;
        }
    }
}