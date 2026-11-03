using UnityEngine;

namespace WeaponModule
{
    /// <summary>
    /// A triangle of a mesh in mesh space, handed to the predicate of
    /// <see cref="WeaponMeshPartExtractor.Split(Mesh, System.Func{MeshTriangle, bool})"/>.
    /// </summary>
    public readonly struct MeshTriangle
    {
        public MeshTriangle(int index, int submesh, Vector3 a, Vector3 b, Vector3 c)
        {
            Index = index;
            Submesh = submesh;
            A = a;
            B = b;
            C = c;
        }

        /// <summary>
        /// Index of the triangle over all submeshes, in submesh order.
        /// </summary>
        public int Index { get; }

        public int Submesh { get; }
        public Vector3 A { get; }
        public Vector3 B { get; }
        public Vector3 C { get; }

        public Vector3 Centroid => (A + B + C) / 3f;
    }
}
