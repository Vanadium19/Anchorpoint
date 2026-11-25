using ComponentsModule;
using UnityEngine;

namespace EnemyModule
{
    /// <summary>
    /// Free 3D flight for the drone: moves toward a destination, hovers when it has none and can dash aside.
    /// </summary>
    public interface IDroneFlightComponent : IPathMoveComponent
    {
        /// <summary>
        /// Whether a dash is in progress. The destination is ignored while it lasts.
        /// </summary>
        bool IsDodging { get; }

        /// <summary>
        /// Whether the drone is flying toward a destination rather than hovering.
        /// </summary>
        bool HasDestination { get; }

        /// <summary>
        /// Dashes <paramref name="distance"/> meters along <paramref name="direction"/> within <paramref name="duration"/> seconds.
        /// </summary>
        void Dodge(Vector3 direction, float distance, float duration);
    }
}
