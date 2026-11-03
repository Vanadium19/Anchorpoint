using System;
using System.Collections.Generic;
using UnityEngine;

namespace WeaponModule
{
    /// <summary>
    /// Splits a weapon mesh into parts by triangle selection, so a part such as a stock barrel or magazine can be
    /// replaced by an attachment. Every part keeps the submeshes (material slots), UVs, normals, tangents, colors,
    /// bone weights and bind poses of the source and holds only the vertices its triangles use. The selected part and
    /// the remainder together contain every triangle of the source exactly once. Nothing is written to disk and no
    /// import setting is changed, so a mesh without Read/Write enabled is read as it is. Blend shapes and non-triangle
    /// submeshes are not supported and throw <see cref="NotSupportedException"/>.
    /// </summary>
    public static class WeaponMeshPartExtractor
    {
        private const float WeldTolerance = 1e-5f;

        /// <summary>
        /// Finds the connected islands of the mesh. Vertices at the same position are welded, so a UV or normal seam
        /// does not cut an island. The result is ordered from the island with the most triangles to the least.
        /// </summary>
        public static List<MeshIsland> FindIslands(Mesh source) => FindIslands(new MeshContent(source));

        /// <summary>
        /// Splits off the given islands, which must come from <see cref="FindIslands(Mesh)"/> of the same mesh.
        /// </summary>
        public static MeshSplit Split(Mesh source, IEnumerable<MeshIsland> selectedIslands)
        {
            if (selectedIslands == null)
                throw new ArgumentNullException(nameof(selectedIslands));

            var content = new MeshContent(source);
            var isSelected = new bool[content.TriangleCount];

            foreach (var island in selectedIslands)
            {
                foreach (var triangle in island.TriangleIndices)
                {
                    if (triangle < 0 || triangle >= isSelected.Length)
                        throw new ArgumentException("An island does not belong to this mesh.", nameof(selectedIslands));

                    isSelected[triangle] = true;
                }
            }

            return Split(source, content, isSelected);
        }

        /// <summary>
        /// Splits off every triangle for which the predicate returns true.
        /// </summary>
        public static MeshSplit Split(Mesh source, Func<MeshTriangle, bool> isSelected)
        {
            if (isSelected == null)
                throw new ArgumentNullException(nameof(isSelected));

            var content = new MeshContent(source);
            var selection = new bool[content.TriangleCount];

            for (var triangle = 0; triangle < selection.Length; triangle++)
                selection[triangle] = isSelected(content.GetTriangle(triangle));

            return Split(source, content, selection);
        }

        /// <summary>
        /// Splits off the triangles whose centroid lies inside an axis-aligned box in mesh space.
        /// </summary>
        public static MeshSplit SplitByBox(Mesh source, Bounds box) =>
            Split(source, triangle => box.Contains(triangle.Centroid));

        private static MeshSplit Split(Mesh source, MeshContent content, bool[] isSelected)
        {
            var selectedCount = 0;

            foreach (var isTriangleSelected in isSelected)
            {
                if (isTriangleSelected)
                    selectedCount++;
            }

            if (selectedCount == 0 || selectedCount == isSelected.Length)
            {
                throw new ArgumentException(
                    $"The selection holds {selectedCount} of {isSelected.Length} triangles of '{source.name}'; " +
                    "a split needs at least one triangle on each side.");
            }

            return new MeshSplit(
                content.Build(source.name + "_Selected", isSelected, true),
                content.Build(source.name + "_Remainder", isSelected, false));
        }

        private static List<MeshIsland> FindIslands(MeshContent content)
        {
            var weldedVertices = WeldVertices(content, out var weldedCount);
            var parents = new int[weldedCount];

            for (var i = 0; i < parents.Length; i++)
                parents[i] = i;

            for (var triangle = 0; triangle < content.TriangleCount; triangle++)
            {
                var first = weldedVertices[content.GetTriangleVertex(triangle, 0)];

                Union(parents, first, weldedVertices[content.GetTriangleVertex(triangle, 1)]);
                Union(parents, first, weldedVertices[content.GetTriangleVertex(triangle, 2)]);
            }

            var groups = GroupTriangles(content, weldedVertices, parents);
            var islands = new List<MeshIsland>(groups.Count);

            groups.Sort(static (left, right) =>
                left.Count != right.Count ? right.Count.CompareTo(left.Count) : left[0].CompareTo(right[0]));

            for (var i = 0; i < groups.Count; i++)
                islands.Add(CreateIsland(content, i, groups[i]));

            return islands;
        }

        private static int[] WeldVertices(MeshContent content, out int weldedCount)
        {
            var lookup = new Dictionary<Vector3Int, int>();
            var weldedVertices = new int[content.VertexCount];

            for (var vertex = 0; vertex < weldedVertices.Length; vertex++)
            {
                var position = content.GetPosition(vertex);
                var key = new Vector3Int(
                    Mathf.RoundToInt(position.x / WeldTolerance),
                    Mathf.RoundToInt(position.y / WeldTolerance),
                    Mathf.RoundToInt(position.z / WeldTolerance));

                if (!lookup.TryGetValue(key, out var welded))
                {
                    welded = lookup.Count;
                    lookup.Add(key, welded);
                }

                weldedVertices[vertex] = welded;
            }

            weldedCount = lookup.Count;

            return weldedVertices;
        }

        private static List<List<int>> GroupTriangles(MeshContent content, int[] weldedVertices, int[] parents)
        {
            var groupByRoot = new Dictionary<int, List<int>>();
            var groups = new List<List<int>>();

            for (var triangle = 0; triangle < content.TriangleCount; triangle++)
            {
                var root = Find(parents, weldedVertices[content.GetTriangleVertex(triangle, 0)]);

                if (!groupByRoot.TryGetValue(root, out var group))
                {
                    group = new List<int>();
                    groupByRoot.Add(root, group);
                    groups.Add(group);
                }

                group.Add(triangle);
            }

            return groups;
        }

        private static MeshIsland CreateIsland(MeshContent content, int index, List<int> triangles)
        {
            var vertices = new HashSet<int>();

            foreach (var triangle in triangles)
            {
                for (var corner = 0; corner < 3; corner++)
                    vertices.Add(content.GetTriangleVertex(triangle, corner));
            }

            var bounds = new Bounds(content.GetPosition(content.GetTriangleVertex(triangles[0], 0)), Vector3.zero);
            var weightByBone = new Dictionary<int, float>();

            foreach (var vertex in vertices)
            {
                bounds.Encapsulate(content.GetPosition(vertex));

                if (content.HasSkin)
                    content.AddBoneWeights(vertex, weightByBone);
            }

            return new MeshIsland(index, vertices.Count, triangles.ToArray(), bounds, FindDominantBone(weightByBone));
        }

        private static int FindDominantBone(Dictionary<int, float> weightByBone)
        {
            var dominantBone = -1;
            var dominantWeight = float.MinValue;

            foreach (var pair in weightByBone)
            {
                if (pair.Value > dominantWeight || (pair.Value == dominantWeight && pair.Key < dominantBone))
                {
                    dominantBone = pair.Key;
                    dominantWeight = pair.Value;
                }
            }

            return dominantBone;
        }

        private static int Find(int[] parents, int item)
        {
            while (parents[item] != item)
            {
                parents[item] = parents[parents[item]];
                item = parents[item];
            }

            return item;
        }

        private static void Union(int[] parents, int first, int second)
        {
            var firstRoot = Find(parents, first);
            var secondRoot = Find(parents, second);

            if (firstRoot != secondRoot)
                parents[Math.Max(firstRoot, secondRoot)] = Math.Min(firstRoot, secondRoot);
        }
    }
}
