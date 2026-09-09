using System;

namespace BuildingModule
{
    /// <summary>
    /// Shared state API for damaging placed buildings.
    /// </summary>
    /// <remarks>
    /// Health is created lazily from the building's BuildingConfig and lives for the scene session only.
    /// </remarks>
    public interface IBuildingDamageService
    {
        /// <summary>
        /// Raised when a building takes damage.
        /// </summary>
        event Action<BuildingView> BuildingDamaged;

        /// <summary>
        /// Raised when a building is broken (health reaches zero).
        /// </summary>
        event Action<BuildingView> BuildingBroken;

        /// <summary>
        /// Determines whether a building is broken.
        /// </summary>
        bool IsBroken(BuildingView building);

        /// <summary>
        /// Attempts to retrieve the health values for a building.
        /// </summary>
        bool TryGetHealth(BuildingView building, out float currentHealth, out float maxHealth);

        /// <summary>
        /// Applies damage to a building.
        /// </summary>
        void ApplyDamage(BuildingView building, float amount);

        /// <summary>
        /// Restores a building to full health.
        /// </summary>
        void Restore(BuildingView building);
    }
}
