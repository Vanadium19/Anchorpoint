using UnityEngine;

namespace ComponentsModule
{
    /// <summary>Supplies the structures an attacker may hit.</summary>
    /// <remarks>A scene with nothing to destroy leaves this unbound, and the attacker then only ever targets entities.</remarks>
    public interface IStructureTargetSource
    {
        /// <summary>Returns the closest valid structure to the given origin.</summary>
        bool TryGetNearest(Vector3 origin, out IStructureTarget target);
    }
}
