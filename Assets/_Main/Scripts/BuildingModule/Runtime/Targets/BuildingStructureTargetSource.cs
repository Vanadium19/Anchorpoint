using ComponentsModule;
using UnityEngine;

namespace BuildingModule
{
    /// <summary>Offers the intact placed buildings as structure targets for attackers.</summary>
    public class BuildingStructureTargetSource : IStructureTargetSource
    {
        private readonly IBuildingRegistry _registry;

        /// <summary>Creates the source with the building registry.</summary>
        public BuildingStructureTargetSource(IBuildingRegistry registry)
        {
            _registry = registry;
        }

        /// <inheritdoc/>
        public bool TryGetNearest(Vector3 origin, out IStructureTarget target)
        {
            target = null;

            var nearestSquaredDistance = float.MaxValue;

            foreach (var building in _registry.Buildings)
            {
                if (building == null || !building.TryGet<BuildingModel>(out var model) || !model.IsAlive)
                    continue;

                var squaredDistance = (building.transform.position - origin).sqrMagnitude;

                if (squaredDistance >= nearestSquaredDistance)
                    continue;

                nearestSquaredDistance = squaredDistance;
                target = new BuildingStructureTarget(building, model);
            }

            return target != null;
        }
    }
}
