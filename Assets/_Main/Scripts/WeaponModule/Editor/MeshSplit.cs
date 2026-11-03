using UnityEngine;

namespace WeaponModule
{
    /// <summary>
    /// The two meshes made by <see cref="WeaponMeshPartExtractor"/>. They are not saved yet: the caller names them
    /// and creates assets. Together they hold every triangle of the source exactly once.
    /// </summary>
    public sealed class MeshSplit
    {
        internal MeshSplit(Mesh selected, Mesh remainder)
        {
            Selected = selected;
            Remainder = remainder;
        }

        /// <summary>
        /// The triangles that were selected.
        /// </summary>
        public Mesh Selected { get; }

        /// <summary>
        /// All other triangles of the source.
        /// </summary>
        public Mesh Remainder { get; }
    }
}
