using System.Collections.Generic;
using UnityEngine;

namespace WeaponModule
{
    /// <summary>
    /// A connected group of triangles of a mesh, found with vertices welded by position.
    /// </summary>
    public sealed class MeshIsland
    {
        private readonly int[] _triangleIndices;

        internal MeshIsland(int index, int vertexCount, int[] triangleIndices, Bounds bounds, int dominantBone)
        {
            Index = index;
            VertexCount = vertexCount;
            Bounds = bounds;
            DominantBone = dominantBone;
            _triangleIndices = triangleIndices;
        }

        /// <summary>
        /// Position of the island in the result of <see cref="WeaponMeshPartExtractor.FindIslands(Mesh)"/>: biggest first.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Number of mesh vertices the triangles of the island use, UV and normal seam duplicates included.
        /// </summary>
        public int VertexCount { get; }

        public int TriangleCount => _triangleIndices.Length;

        /// <summary>
        /// Bounds in mesh space.
        /// </summary>
        public Bounds Bounds { get; }

        /// <summary>
        /// Bone with the largest summed weight over the vertices of the island, or -1 for a mesh without skin weights.
        /// </summary>
        public int DominantBone { get; }

        /// <summary>
        /// Indices of the triangles of the island over all submeshes, in submesh order.
        /// </summary>
        public IReadOnlyList<int> TriangleIndices => _triangleIndices;
    }
}
