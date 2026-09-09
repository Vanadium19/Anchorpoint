using BuildingModule;

namespace RandomEventsModule
{
    /// <summary>Met while at least one building is registered.</summary>
    public class HasBuildingsCondition : IRandomEventCondition
    {
        private readonly IBuildingRegistry _registry;

        /// <summary>Creates the condition over the building registry.</summary>
        public HasBuildingsCondition(IBuildingRegistry registry)
        {
            _registry = registry;
        }

        /// <inheritdoc/>
        public bool IsMet() => _registry.Buildings.Count > 0;
    }
}
