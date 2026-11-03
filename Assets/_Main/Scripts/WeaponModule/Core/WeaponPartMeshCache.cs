using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace WeaponModule
{
    /// <summary>
    /// Keeps the meshes made by part operations, by source mesh, operation and mount placement. A mesh is created
    /// on first use and reused afterwards, by every weapon view of the same weapon. The cache is cleared when the
    /// domain reloads, when play mode starts and when an operation asset is changed in the Editor.
    /// </summary>
    public static class WeaponPartMeshCache
    {
        private const float MatrixQuantum = 10000f;
        private const ulong FnvOffset = 14695981039346656037UL;
        private const ulong FnvPrime = 1099511628211UL;

        private static readonly Dictionary<(int Source, int Operation, ulong Placement), Mesh> Meshes = new();

        public static int Count => Meshes.Count;

        /// <summary>
        /// Returns the operated mesh, creating it with <see cref="WeaponPartOperation.Apply"/> on first use.
        /// </summary>
        /// <param name="operation">The operation to apply.</param>
        /// <param name="context">The stock part. Placements closer than 0.1 mm share one mesh.</param>
        /// <returns>The cached mesh.</returns>
        public static Mesh GetOrCreate(WeaponPartOperation operation, in WeaponPartContext context)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            if (context.Source == null)
                throw new ArgumentException("The context has no source mesh.", nameof(context));

            var key = (context.Source.GetInstanceID(), operation.GetInstanceID(), Fingerprint(in context));

            if (Meshes.TryGetValue(key, out var mesh) && mesh != null)
                return mesh;

            mesh = operation.Apply(in context);

            if (mesh == null)
                throw new InvalidOperationException($"Operation '{operation.name}' returned no mesh.");

            Meshes[key] = mesh;

            return mesh;
        }

        /// <summary>
        /// Forgets the meshes of one operation, so the next use creates them again. Meshes that are still shown
        /// stay alive until nothing references them.
        /// </summary>
        /// <param name="operation">The operation whose asset has changed.</param>
        public static void Invalidate(WeaponPartOperation operation)
        {
            if (operation == null || Meshes.Count == 0)
                return;

            var operationId = operation.GetInstanceID();
            var stale = new List<(int Source, int Operation, ulong Placement)>();

            foreach (var key in Meshes.Keys)
            {
                if (key.Operation == operationId)
                    stale.Add(key);
            }

            foreach (var key in stale)
                Meshes.Remove(key);
        }

        /// <summary>
        /// Destroys every cached mesh.
        /// </summary>
        public static void Clear()
        {
            foreach (var mesh in Meshes.Values)
            {
                if (mesh == null)
                    continue;

                if (Application.isPlaying)
                    Object.Destroy(mesh);
                else
                    Object.DestroyImmediate(mesh);
            }

            Meshes.Clear();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() => Meshes.Clear();

        private static ulong Fingerprint(in WeaponPartContext context)
        {
            var matrix = context.MeshToMount;
            var hash = FnvOffset;

            for (var row = 0; row < 3; row++)
            {
                for (var column = 0; column < 4; column++)
                    hash = Mix(hash, matrix[row, column]);
            }

            hash = Mix(hash, context.ClearanceAbove);

            return Mix(hash, context.ClearanceBelow);
        }

        private static ulong Mix(ulong hash, float value)
        {
            var quantized = (long)Mathf.Round(value * MatrixQuantum);

            unchecked
            {
                return (hash ^ (ulong)quantized) * FnvPrime;
            }
        }
    }
}
