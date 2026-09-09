using UnityEngine;

namespace ComponentsModule
{
    /// <summary>A static object an attacker can hit instead of an entity: a building, a barricade, a gate.</summary>
    /// <remarks>The implementation belongs to the module that owns such objects, so an attacker never depends on it. Once the object is gone or already ruined, <see cref="IsValid"/> is false.</remarks>
    public interface IStructureTarget
    {
        /// <summary>World-space extents of the structure, used both to aim at it and to measure the distance to its surface.</summary>
        Bounds Bounds { get; }

        /// <summary>Whether the structure can still be attacked.</summary>
        bool IsValid { get; }

        /// <summary>Applies damage through the owning module's state API.</summary>
        void TakeDamage(float amount);
    }
}
