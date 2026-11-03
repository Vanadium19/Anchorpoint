using UnityEngine;

namespace WeaponModule
{
    /// <summary>
    /// A kind of change to the stock part of a weapon that a modification can apply, for example a thicker or a
    /// longer part. An operation is pure on mesh data and never reads the scene. A new modification is a new
    /// asset of an existing operation; code is needed only for a new kind of operation.
    /// </summary>
    public abstract class WeaponPartOperation : ScriptableObject
    {
        /// <summary>
        /// Creates a new mesh from the stock mesh. The result has the same vertex count, triangles, submeshes,
        /// UVs, bone weights and bind poses as the source, only the positions and the shading differ.
        /// </summary>
        /// <param name="context">The stock mesh and where the mount lies relative to it.</param>
        /// <returns>The new mesh. The cache keeps it.</returns>
        public abstract Mesh Apply(in WeaponPartContext context);

#if UNITY_EDITOR
        protected virtual void OnValidate() => WeaponPartMeshCache.Invalidate(this);
#endif
    }
}
