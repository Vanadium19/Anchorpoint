using System;
using System.Collections.Generic;

namespace BuildingModule
{
    /// <summary>Default <see cref="IBuildingDamageService"/>: keeps a health entry per placed building.</summary>
    /// <remarks>A building not damaged yet has no entry; the first call creates one from its <see cref="BuildingConfig"/>.</remarks>
    public class BuildingDamageService : IBuildingDamageService
    {
        private const float FallbackMaxHealth = 100f;

        private readonly BuildingCatalog _catalog;
        private readonly Dictionary<BuildingView, BuildingHealth> _healthByBuilding = new();

        /// <inheritdoc/>
        public event Action<BuildingView> BuildingDamaged;

        /// <inheritdoc/>
        public event Action<BuildingView> BuildingBroken;

        /// <summary>Creates the service over the catalog that holds building health values.</summary>
        public BuildingDamageService(BuildingCatalog catalog)
        {
            _catalog = catalog;
        }

        /// <inheritdoc/>
        public bool IsBroken(BuildingView building) => building != null && GetHealth(building).IsBroken;

        /// <inheritdoc/>
        public bool TryGetHealth(BuildingView building, out float currentHealth, out float maxHealth)
        {
            if (building == null)
            {
                currentHealth = 0f;
                maxHealth = 0f;

                return false;
            }

            var health = GetHealth(building);
            currentHealth = health.CurrentHealth;
            maxHealth = health.MaxHealth;

            return true;
        }

        /// <inheritdoc/>
        public void ApplyDamage(BuildingView building, float amount)
        {
            if (building == null)
                return;

            var isBroken = GetHealth(building).TakeDamage(amount);

            BuildingDamaged?.Invoke(building);

            if (isBroken)
                BuildingBroken?.Invoke(building);
        }

        /// <inheritdoc/>
        public void Restore(BuildingView building)
        {
            if (building == null)
                return;

            GetHealth(building).Restore();
        }

        private BuildingHealth GetHealth(BuildingView building)
        {
            if (_healthByBuilding.TryGetValue(building, out var existingHealth))
                return existingHealth;

            var health = new BuildingHealth(GetMaxHealth(building));
            _healthByBuilding[building] = health;

            return health;
        }

        private float GetMaxHealth(BuildingView building) =>
            _catalog.TryGetConfig(building.BuildingConfigId, out var config) ? config.MaxHealth : FallbackMaxHealth;
    }
}
