using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace WeaponModule
{
    /// <summary>
    /// Shared mesh plumbing of the part operations: reads the vertices of a stock mesh in the space of the mount
    /// and builds the operated copy of the mesh, keeping everything but the positions and the shading of the source.
    /// </summary>
    public static class WeaponPartMeshFactory
    {
        private const float MinDirectionLength = 1e-12f;
        private const float MinSine = 1e-7f;
        private const float MinDeterminant = 1e-12f;

        /// <summary>
        /// Reads the vertices of the stock mesh and transforms them into the space of the mount.
        /// </summary>
        /// <param name="context">The stock part.</param>
        /// <returns>A new array with one position per vertex of the source mesh.</returns>
        public static Vector3[] ReadPositions(in WeaponPartContext context)
        {
            var source = RequireReadable(context.Source);
            var positions = source.vertices;
            var meshToMount = context.MeshToMount;

            for (var i = 0; i < positions.Length; i++)
                positions[i] = meshToMount.MultiplyPoint3x4(positions[i]);

            return positions;
        }

        /// <summary>
        /// Reads the triangle indices of every triangle submesh of the stock mesh.
        /// </summary>
        /// <param name="context">The stock part.</param>
        /// <returns>The indices into the vertices of the source mesh, three per triangle.</returns>
        public static int[] ReadTriangles(in WeaponPartContext context) => ReadTriangles(RequireReadable(context.Source));

        /// <summary>
        /// Creates a copy of the stock mesh with new positions. Normals and tangents are rotated by the change of
        /// the faces around each vertex, so flat faces stay flat and smooth surfaces stay smooth. Bounds are
        /// recalculated.
        /// </summary>
        /// <param name="context">The stock part.</param>
        /// <param name="mountPositions">New positions in the space of the mount, one per source vertex.</param>
        /// <param name="name">Name of the new mesh.</param>
        /// <returns>The new mesh.</returns>
        public static Mesh Create(in WeaponPartContext context, Vector3[] mountPositions, string name)
        {
            var source = RequireReadable(context.Source);

            if (mountPositions == null || mountPositions.Length != source.vertexCount)
            {
                throw new ArgumentException(
                    $"Mesh '{source.name}' has {source.vertexCount} vertices, got {mountPositions?.Length ?? 0} positions.",
                    nameof(mountPositions));
            }

            var meshToMount = context.MeshToMount;

            if (Mathf.Abs(meshToMount.determinant) < MinDeterminant)
                throw new InvalidOperationException($"The mesh to mount matrix of '{source.name}' is not invertible.");

            var mountToMesh = meshToMount.inverse;
            var oldPositions = source.vertices;
            var newPositions = new Vector3[mountPositions.Length];

            for (var i = 0; i < newPositions.Length; i++)
            {
                newPositions[i] = mountToMesh.MultiplyPoint3x4(mountPositions[i]);

                if (IsInvalid(newPositions[i]))
                    throw new InvalidOperationException($"A part operation produced an invalid vertex in '{source.name}'.");
            }

            var mesh = Object.Instantiate(source);
            var triangles = ReadTriangles(source);

            mesh.name = name;
            mesh.vertices = newPositions;

            var normals = source.normals;

            if (normals.Length == newPositions.Length)
            {
                RotateDirections(oldPositions, newPositions, triangles, normals);
                mesh.normals = normals;
            }

            var tangents = source.tangents;

            if (tangents.Length == newPositions.Length)
                mesh.tangents = RotateTangents(oldPositions, newPositions, triangles, tangents);

            mesh.RecalculateBounds();

            return mesh;
        }

        /// <summary>
        /// Rotates each direction by the rotation that the face normal at its vertex has made between the old and
        /// the new positions. The face normal of a vertex is the area weighted sum over the triangles that use it.
        /// </summary>
        /// <param name="oldPositions">Positions before the operation.</param>
        /// <param name="newPositions">Positions after the operation.</param>
        /// <param name="triangles">Triangle indices, three per triangle.</param>
        /// <param name="directions">Normals or tangent directions, rotated in place.</param>
        public static void RotateDirections(
            Vector3[] oldPositions,
            Vector3[] newPositions,
            int[] triangles,
            Vector3[] directions)
        {
            var oldFaces = AccumulateFaceNormals(oldPositions, triangles);
            var newFaces = AccumulateFaceNormals(newPositions, triangles);

            for (var i = 0; i < directions.Length; i++)
                directions[i] = Rotate(directions[i], oldFaces[i], newFaces[i]);
        }

        private static Mesh RequireReadable(Mesh mesh)
        {
            if (mesh == null)
                throw new ArgumentNullException(nameof(mesh), "A part operation needs a source mesh.");

            if (!mesh.isReadable)
            {
                throw new InvalidOperationException(
                    $"Mesh '{mesh.name}' is not readable. Enable Read/Write in its import settings.");
            }

            return mesh;
        }

        private static bool IsInvalid(Vector3 value) =>
            float.IsNaN(value.x) || float.IsNaN(value.y) || float.IsNaN(value.z)
            || float.IsInfinity(value.x) || float.IsInfinity(value.y) || float.IsInfinity(value.z);

        private static int[] ReadTriangles(Mesh mesh)
        {
            var triangles = new List<int>();

            for (var i = 0; i < mesh.subMeshCount; i++)
            {
                if (mesh.GetTopology(i) == MeshTopology.Triangles)
                    triangles.AddRange(mesh.GetTriangles(i));
            }

            return triangles.ToArray();
        }

        private static Vector4[] RotateTangents(
            Vector3[] oldPositions,
            Vector3[] newPositions,
            int[] triangles,
            Vector4[] tangents)
        {
            var directions = new Vector3[tangents.Length];

            for (var i = 0; i < directions.Length; i++)
                directions[i] = tangents[i];

            RotateDirections(oldPositions, newPositions, triangles, directions);

            for (var i = 0; i < directions.Length; i++)
                tangents[i] = new Vector4(directions[i].x, directions[i].y, directions[i].z, tangents[i].w);

            return tangents;
        }

        private static Vector3[] AccumulateFaceNormals(Vector3[] positions, int[] triangles)
        {
            var normals = new Vector3[positions.Length];

            for (var i = 0; i + 2 < triangles.Length; i += 3)
            {
                var a = triangles[i];
                var b = triangles[i + 1];
                var c = triangles[i + 2];
                var faceNormal = Vector3.Cross(positions[b] - positions[a], positions[c] - positions[a]);

                normals[a] += faceNormal;
                normals[b] += faceNormal;
                normals[c] += faceNormal;
            }

            return normals;
        }

        private static Vector3 Rotate(Vector3 direction, Vector3 from, Vector3 to)
        {
            if (from.sqrMagnitude < MinDirectionLength || to.sqrMagnitude < MinDirectionLength)
                return direction;

            from.Normalize();
            to.Normalize();

            var axis = Vector3.Cross(from, to);
            var sine = axis.magnitude;

            if (sine < MinSine)
                return direction;

            axis /= sine;

            var cosine = Vector3.Dot(from, to);
            var rotated = direction * cosine
                + Vector3.Cross(axis, direction) * sine
                + axis * (Vector3.Dot(axis, direction) * (1f - cosine));

            return IsInvalid(rotated) ? direction : rotated;
        }
    }
}
