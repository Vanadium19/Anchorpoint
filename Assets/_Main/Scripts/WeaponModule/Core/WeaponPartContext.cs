using UnityEngine;

namespace WeaponModule
{
    /// <summary>
    /// Input of a <see cref="WeaponPartOperation"/>: a stock part mesh, where the socket mount lies relative to it
    /// and how much room the weapon leaves around the mount axis.
    /// </summary>
    public readonly struct WeaponPartContext
    {
        /// <summary>
        /// Creates the context of one stock part.
        /// </summary>
        /// <param name="source">The readable stock mesh. It is never modified.</param>
        /// <param name="meshToMount">Transforms points from the space of the mesh into the space of the mount.</param>
        /// <param name="clearanceAbove">Room in metres above the mount axis, 0 for unlimited.</param>
        /// <param name="clearanceBelow">Room in metres below the mount axis, 0 for unlimited.</param>
        public WeaponPartContext(Mesh source, Matrix4x4 meshToMount, float clearanceAbove, float clearanceBelow)
        {
            Source = source;
            MeshToMount = meshToMount;
            ClearanceAbove = clearanceAbove;
            ClearanceBelow = clearanceBelow;
        }

        public Mesh Source { get; }

        public Matrix4x4 MeshToMount { get; }

        public float ClearanceAbove { get; }

        public float ClearanceBelow { get; }
    }
}
