using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine;

namespace WeaponModule.Tests
{
    public class WeaponMeshPartExtractorTests
    {
        private const int TrianglesPerCube = 12;
        private const int VerticesPerCube = 24;

        private static readonly Vector3 SecondCubeCenter = new(5f, 0f, 0f);

        private readonly List<UnityEngine.Object> _created = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var created in _created)
                UnityEngine.Object.DestroyImmediate(created);

            _created.Clear();
        }

        [Test]
        public void FindIslands_TwoCubes_ReportsBothIslands()
        {
            var mesh = CreateTwoCubeMesh();

            var islands = WeaponMeshPartExtractor.FindIslands(mesh);

            Assert.AreEqual(2, islands.Count);

            foreach (var island in islands)
            {
                Assert.AreEqual(TrianglesPerCube, island.TriangleCount);
                Assert.AreEqual(VerticesPerCube, island.VertexCount);
                Assert.AreEqual(Vector3.one, island.Bounds.size);
            }

            Assert.AreEqual(0, islands[0].Index);
            Assert.AreEqual(1, islands[1].Index);
            Assert.AreEqual(0, islands[0].DominantBone);
            Assert.AreEqual(1, islands[1].DominantBone);
            Assert.AreEqual(Vector3.zero, islands[0].Bounds.center);
            Assert.AreEqual(SecondCubeCenter, islands[1].Bounds.center);
        }

        [Test]
        public void Split_ByIsland_KeepsEveryTriangleExactlyOnce()
        {
            var mesh = CreateTwoCubeMesh();
            var islands = WeaponMeshPartExtractor.FindIslands(mesh);

            var split = Track(WeaponMeshPartExtractor.Split(mesh, new[] { islands[1] }));

            Assert.AreEqual(TrianglesPerCube, CountTriangles(split.Selected));
            Assert.AreEqual(TrianglesPerCube, CountTriangles(split.Remainder));

            var combined = GetTriangleSignatures(split.Selected);

            combined.AddRange(GetTriangleSignatures(split.Remainder));

            CollectionAssert.AreEquivalent(GetTriangleSignatures(mesh), combined);
        }

        [Test]
        public void Split_ByIsland_KeepsSubmeshesAttributesAndSkin()
        {
            var mesh = CreateTwoCubeMesh();
            var islands = WeaponMeshPartExtractor.FindIslands(mesh);

            var split = Track(WeaponMeshPartExtractor.Split(mesh, new[] { islands[1] }));

            foreach (var part in new[] { split.Selected, split.Remainder })
            {
                Assert.AreEqual(mesh.subMeshCount, part.subMeshCount);
                Assert.AreEqual(VerticesPerCube, part.vertexCount);
                Assert.AreEqual(mesh.bindposeCount, part.bindposeCount);
                Assert.AreEqual(part.vertexCount, part.normals.Length);
                Assert.AreEqual(part.vertexCount, part.tangents.Length);
                Assert.AreEqual(part.vertexCount, part.colors.Length);
                Assert.AreEqual(part.vertexCount, part.uv.Length);
                Assert.AreEqual(part.vertexCount, part.GetBonesPerVertex().Length);
            }

            Assert.AreEqual(0, split.Selected.GetIndices(0).Length);
            Assert.AreEqual(TrianglesPerCube * 3, split.Selected.GetIndices(1).Length);
            Assert.AreEqual(TrianglesPerCube * 3, split.Remainder.GetIndices(0).Length);
            Assert.AreEqual(0, split.Remainder.GetIndices(1).Length);
            Assert.AreEqual(1, split.Selected.GetAllBoneWeights()[0].boneIndex);
            Assert.AreEqual(0, split.Remainder.GetAllBoneWeights()[0].boneIndex);
            Assert.AreEqual(SecondCubeCenter, split.Selected.bounds.center);
        }

        [Test]
        public void SplitByBox_SelectsTrianglesInsideTheBox()
        {
            var mesh = CreateTwoCubeMesh();
            var box = new Bounds(SecondCubeCenter, Vector3.one * 2f);

            var split = Track(WeaponMeshPartExtractor.SplitByBox(mesh, box));

            Assert.AreEqual(TrianglesPerCube, CountTriangles(split.Selected));
            Assert.AreEqual(SecondCubeCenter, split.Selected.bounds.center);
            Assert.AreEqual(Vector3.zero, split.Remainder.bounds.center);
        }

        [Test]
        public void Split_ByPredicate_MatchesSelectionOnSubmesh()
        {
            var mesh = CreateTwoCubeMesh();

            var split = Track(WeaponMeshPartExtractor.Split(mesh, triangle => triangle.Submesh == 1));

            Assert.AreEqual(TrianglesPerCube, CountTriangles(split.Selected));
            Assert.AreEqual(SecondCubeCenter, split.Selected.bounds.center);
        }

        [Test]
        public void Split_SelectingNothingOrEverything_Throws()
        {
            var mesh = CreateTwoCubeMesh();

            Assert.Throws<ArgumentException>(() => WeaponMeshPartExtractor.Split(mesh, _ => false));
            Assert.Throws<ArgumentException>(() => WeaponMeshPartExtractor.Split(mesh, _ => true));
        }

        private static int CountTriangles(Mesh mesh)
        {
            var count = 0;

            for (var submesh = 0; submesh < mesh.subMeshCount; submesh++)
                count += mesh.GetIndices(submesh).Length / 3;

            return count;
        }

        private static List<string> GetTriangleSignatures(Mesh mesh)
        {
            var signatures = new List<string>();
            var vertices = mesh.vertices;

            for (var submesh = 0; submesh < mesh.subMeshCount; submesh++)
            {
                var indices = mesh.GetIndices(submesh);

                for (var i = 0; i < indices.Length; i += 3)
                {
                    signatures.Add($"{submesh}|{vertices[indices[i]]:F4}|{vertices[indices[i + 1]]:F4}|" +
                                   $"{vertices[indices[i + 2]]:F4}");
                }
            }

            return signatures;
        }

        private static void AddCube(Vector3 center, int bone, List<Vector3> positions, List<Vector3> normals,
            List<Vector4> tangents, List<Vector2> uvs, List<Color> colors, List<int> triangles,
            List<byte> bonesPerVertex, List<BoneWeight1> boneWeights)
        {
            var faceNormals = new[]
            {
                Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back
            };

            foreach (var normal in faceNormals)
            {
                var tangent = Mathf.Abs(Vector3.Dot(normal, Vector3.up)) > 0.9f
                    ? Vector3.right
                    : Vector3.Cross(Vector3.up, normal).normalized;
                var bitangent = Vector3.Cross(normal, tangent);
                var first = positions.Count;

                AddCorner(-1f, -1f);
                AddCorner(1f, -1f);
                AddCorner(1f, 1f);
                AddCorner(-1f, 1f);

                triangles.AddRange(new[] { first, first + 1, first + 2, first, first + 2, first + 3 });

                void AddCorner(float u, float v)
                {
                    positions.Add(center + normal * 0.5f + tangent * (u * 0.5f) + bitangent * (v * 0.5f));
                    normals.Add(normal);
                    tangents.Add(new Vector4(tangent.x, tangent.y, tangent.z, 1f));
                    uvs.Add(new Vector2((u + 1f) * 0.5f, (v + 1f) * 0.5f));
                    colors.Add(Color.white);
                    bonesPerVertex.Add(1);
                    boneWeights.Add(new BoneWeight1 { boneIndex = bone, weight = 1f });
                }
            }
        }

        private Mesh CreateTwoCubeMesh()
        {
            var positions = new List<Vector3>();
            var normals = new List<Vector3>();
            var tangents = new List<Vector4>();
            var uvs = new List<Vector2>();
            var colors = new List<Color>();
            var firstTriangles = new List<int>();
            var secondTriangles = new List<int>();
            var bonesPerVertex = new List<byte>();
            var boneWeights = new List<BoneWeight1>();

            AddCube(Vector3.zero, 0, positions, normals, tangents, uvs, colors, firstTriangles, bonesPerVertex,
                boneWeights);
            AddCube(SecondCubeCenter, 1, positions, normals, tangents, uvs, colors, secondTriangles, bonesPerVertex,
                boneWeights);

            var mesh = new Mesh { name = "TwoCubes" };

            _created.Add(mesh);
            mesh.SetVertices(positions);
            mesh.SetNormals(normals);
            mesh.SetTangents(tangents);
            mesh.SetUVs(0, uvs);
            mesh.SetColors(colors);
            mesh.subMeshCount = 2;
            mesh.SetTriangles(firstTriangles, 0);
            mesh.SetTriangles(secondTriangles, 1);
            mesh.bindposes = new[] { Matrix4x4.identity, Matrix4x4.identity };

            using var nativeBonesPerVertex = new NativeArray<byte>(bonesPerVertex.ToArray(), Allocator.Temp);
            using var nativeBoneWeights = new NativeArray<BoneWeight1>(boneWeights.ToArray(), Allocator.Temp);

            mesh.SetBoneWeights(nativeBonesPerVertex, nativeBoneWeights);

            return mesh;
        }

        private MeshSplit Track(MeshSplit split)
        {
            _created.Add(split.Selected);
            _created.Add(split.Remainder);

            return split;
        }
    }
}
