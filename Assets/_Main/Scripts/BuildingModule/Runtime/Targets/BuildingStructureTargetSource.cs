using ComponentsModule;
using UnityEngine;

namespace BuildingModule
{
    /// <summary>Offers the intact placed buildings as structure targets for attackers.</summary>
    public class BuildingStructureTargetSource : IStructureTargetSource
    {
        private readonly IBuildingRegistry _registry;
        private readonly IBuildingDamageService _damageService;

        /// <summary>Creates the source with the building registry and damage service.</summary>
        public BuildingStructureTargetSource(IBuildingRegistry registry, IBuildingDamageService damageService)
        {
            _registry = registry;
            _damageService = damageService;
        }

        /// <inheritdoc/>
        public bool TryGetNearest(Vector3 origin, out IStructureTarget target)
        {
            target = null;

            var nearestSquaredDistance = float.MaxValue;

            foreach (var building in _registry.Buildings)
            {
                if (building == null || _damageService.IsBroken(building))
                    continue;

                var squaredDistance = (building.transform.position - origin).sqrMagnitude;

                if (squaredDistance >= nearestSquaredDistance)
                    continue;

                nearestSquaredDistance = squaredDistance;
                target = new BuildingStructureTarget(building, _damageService);
            }

            return target != null;
        }
    }
}
