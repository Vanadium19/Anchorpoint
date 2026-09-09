using ComponentsModule;
using UnityEngine;

namespace BuildingModule
{
    /// <summary>One placed building exposed as a structure target.</summary>
    /// <remarks>The bounds are taken once at creation, because a placed building does not move.</remarks>
    public class BuildingStructureTarget : IStructureTarget
    {
        private readonly BuildingView _building;
        private readonly IBuildingDamageService _damageService;
        private readonly Bounds _bounds;

        /// <summary>Creates the target for the given building.</summary>
        public BuildingStructureTarget(BuildingView building, IBuildingDamageService damageService)
        {
            _building = building;
            _damageService = damageService;
            _bounds = CreateBounds(building);
        }

        /// <inheritdoc/>
        public Bounds Bounds => _bounds;

        /// <inheritdoc/>
        public bool IsValid => _building != null && !_damageService.IsBroken(_building);

        /// <inheritdoc/>
        public void TakeDamage(float amount) => _damageService.ApplyDamage(_building, amount);

        private static Bounds CreateBounds(BuildingView building)
        {
            var renderers = building.GetAllMeshRenderers();

            if (renderers.Length == 0)
                return new Bounds(building.transform.position, Vector3.one);

            var bounds = renderers[0].bounds;

            for (var index = 1; index < renderers.Length; index++)
                bounds.Encapsulate(renderers[index].bounds);

            return bounds;
        }
    }
}
