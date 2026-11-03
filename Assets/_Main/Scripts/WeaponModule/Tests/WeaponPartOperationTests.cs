using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace WeaponModule.Tests
{
    public class WeaponPartOperationTests
    {
        private const float Tolerance = 1e-5f;
        private const float ArcTolerance = 2e-3f;
        private const float ArcRadius = 0.3f;
        private const float ArcHalfThickness = 0.01f;
        private const float ArcHalfWidth = 0.02f;

        private readonly List<Object> _created = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var created in _created)
                Object.DestroyImmediate(created);

            _created.Clear();
            WeaponPartMeshCache.Clear();
        }

        [Test]
        public void Thicken_ScalesDistanceFromAxisAndKeepsAxialPosition()
        {
            var positions = new[] { new Vector3(0.1f, 0f, 0.5f), new Vector3(0f, -0.2f, 1f), new Vector3(0.03f, 0.04f, -1f) };

            RadialThickenOperation.Thicken(positions, 1.5f, 0f, 0f, 0f, 0f);

            AssertVector(new Vector3(0.15f, 0f, 0.5f), positions[0]);
            AssertVector(new Vector3(0f, -0.3f, 1f), positions[1]);
            AssertVector(new Vector3(0.045f, 0.06f, -1f), positions[2]);
        }

        [Test]
        public void Thicken_VerticesInsideMinRadius_StayWhereTheyAre()
        {
            var bore = new Vector3(0.008f, 0f, 0.2f);
            var wall = new Vector3(0.02f, 0f, 0.2f);
            var positions = new[] { bore, wall };

            RadialThickenOperation.Thicken(positions, 2f, 0.015f, 0f, 0f, 0f);

            AssertVector(bore, positions[0]);
            AssertVector(new Vector3(0.04f, 0f, 0.2f), positions[1]);
        }

        [Test]
        public void Thicken_ClearanceAbove_StopsGrowthAtTheClearance()
        {
            var positions = new[]
            {
                new Vector3(0f, 0.04f, 0f),
                new Vector3(0.04f, 0f, 0f),
                new Vector3(0f, 0.06f, 0f)
            };

            RadialThickenOperation.Thicken(positions, 2f, 0f, 0f, 0.05f, 0f);

            AssertVector(new Vector3(0f, 0.05f, 0f), positions[0]);
            AssertVector(new Vector3(0.08f, 0f, 0f), positions[1]);
            AssertVector(new Vector3(0f, 0.06f, 0f), positions[2]);
        }

        [Test]
        public void Thicken_ClearanceBelow_StopsGrowthAtTheClearance()
        {
            var positions = new[] { new Vector3(0f, -0.04f, 0f), new Vector3(0f, 0.04f, 0f) };

            RadialThickenOperation.Thicken(positions, 2f, 0f, 0f, 0f, 0.05f);

            AssertVector(new Vector3(0f, -0.05f, 0f), positions[0]);
            AssertVector(new Vector3(0f, 0.08f, 0f), positions[1]);
        }

        [Test]
        public void Thicken_ScaleBelowOne_ShrinksTowardsTheAxis()
        {
            var positions = new[] { new Vector3(0f, 0.04f, 0f) };

            RadialThickenOperation.Thicken(positions, 0.5f, 0f, 0f, 0.01f, 0.01f);

            AssertVector(new Vector3(0f, 0.02f, 0f), positions[0]);
        }

        [Test]
        public void Thicken_AxialFalloff_KeepsTheEndsAndScalesTheMiddle()
        {
            var positions = new List<Vector3>();

            for (var i = 0; i <= 10; i++)
                positions.Add(new Vector3(0.02f, 0f, i * 0.1f));

            var array = positions.ToArray();

            RadialThickenOperation.Thicken(array, 2f, 0f, 0.3f, 0f, 0f);

            Assert.AreEqual(0.02f, array[0].x, Tolerance);
            Assert.AreEqual(0.02f, array[10].x, Tolerance);
            Assert.AreEqual(0.04f, array[5].x, Tolerance);
            Assert.Greater(array[2].x, array[1].x);
            Assert.Greater(array[3].x, array[2].x);
        }

        [Test]
        public void Thicken_VertexOnTheAxis_StaysFinite()
        {
            var positions = new[] { new Vector3(0f, 0f, 0.3f) };

            RadialThickenOperation.Thicken(positions, 3f, 0f, 0.2f, 0.01f, 0.01f);

            AssertVector(new Vector3(0f, 0f, 0.3f), positions[0]);
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        public void Thicken_InvalidScale_Throws(float scale)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => RadialThickenOperation.Thicken(new[] { Vector3.one }, scale, 0f, 0f, 0f, 0f));
        }

        [Test]
        public void Extend_CurvedPart_LengthensAlongTheArcAndKeepsTheCrossSection()
        {
            var original = CreateArcPart(0.8f, 8);
            var positions = original.ToArray();

            CurveExtendOperation.Extend(positions, 1.5f, 0.25f, 0f, 0f);

            for (var slice = 1; slice <= 8; slice++)
            {
                var before = GetSlice(original, slice);
                var after = GetSlice(positions, slice);

                AssertSameCrossSection(before, after);
            }

            var farCenter = GetSliceCenter(positions, 8);
            var expected = GetArcPoint(0.8f * 1.5f);

            Assert.AreEqual(expected.x, farCenter.z, ArcTolerance);
            Assert.AreEqual(expected.y, -farCenter.y, ArcTolerance);
        }

        [Test]
        public void Extend_CurvedPart_StretchesBetweenTheExitAndTheRigidEnd()
        {
            var original = CreateArcPart(0.8f, 8);
            var positions = original.ToArray();

            CurveExtendOperation.Extend(positions, 1.5f, 0.25f, 0f, 0f);

            var stretchBefore = Vector3.Distance(GetSliceCenter(original, 2), GetSliceCenter(original, 3));
            var stretchAfter = Vector3.Distance(GetSliceCenter(positions, 2), GetSliceCenter(positions, 3));
            var rigidBefore = Vector3.Distance(GetSliceCenter(original, 7), GetSliceCenter(original, 8));
            var rigidAfter = Vector3.Distance(GetSliceCenter(positions, 7), GetSliceCenter(positions, 8));

            Assert.AreEqual(1f + 0.4f / 0.6f, stretchAfter / stretchBefore, 0.05f);
            Assert.AreEqual(rigidBefore, rigidAfter, ArcTolerance);
        }

        [Test]
        public void Extend_CurvedPart_NothingAboveTheExitMoves()
        {
            var original = CreateArcPart(0.8f, 8);
            var positions = original.ToArray();

            CurveExtendOperation.Extend(positions, 2f, 0.1f, 0f, 0f);

            for (var i = 0; i < original.Length; i++)
            {
                if (original[i].y >= 0f)
                    Assert.AreEqual(original[i], positions[i], $"Vertex {i} above the exit moved.");
            }
        }

        [Test]
        public void Extend_CurvedPart_StartsSmoothlyAtTheExit()
        {
            var original = CreateArcPart(0.8f, 8);
            var positions = original.ToArray();

            CurveExtendOperation.Extend(positions, 1.5f, 0.25f, 0f, 0f);

            var exitSliceBefore = GetSliceCenter(original, 0);
            var exitSliceAfter = GetSliceCenter(positions, 0);
            var firstSliceBefore = GetSliceCenter(original, 1);
            var firstSliceAfter = GetSliceCenter(positions, 1);
            var expectedShift = ArcRadius * 0.1f * (0.5f * 0.8f / 0.6f);

            Assert.AreEqual(exitSliceBefore, exitSliceAfter);
            Assert.AreEqual(expectedShift, Vector3.Distance(firstSliceBefore, firstSliceAfter), ArcTolerance);
        }

        [Test]
        public void Extend_StraightPart_ExtendsInAStraightLine()
        {
            var original = CreateStraightPart(0f);
            var positions = original.ToArray();

            CurveExtendOperation.Extend(positions, 1.5f, 0.2f, 0f, 0f);

            var lowestBefore = original.Min(position => position.y);
            var lowestAfter = positions.Min(position => position.y);

            Assert.AreEqual(lowestBefore * 1.5f, lowestAfter, Tolerance);

            for (var i = 0; i < original.Length; i++)
            {
                Assert.AreEqual(original[i].x, positions[i].x, Tolerance);
                Assert.AreEqual(original[i].z, positions[i].z, Tolerance);
                Assert.LessOrEqual(positions[i].y, original[i].y + Tolerance);
            }
        }

        [Test]
        public void Extend_TiltedStraightPart_ExtendsAlongItsOwnDirection()
        {
            var original = CreateStraightPart(0.25f);
            var positions = original.ToArray();

            CurveExtendOperation.Extend(positions, 1.5f, 0.2f, 0f, 0f);

            var lowestBefore = original.OrderBy(position => position.y).First();
            var lowestAfter = positions.OrderBy(position => position.y).First();
            var travel = lowestAfter - lowestBefore;

            Assert.AreEqual(-0.25f, travel.z / travel.y, 1e-3f);
            AssertSameCrossSection(original.Skip(16).Take(4).ToArray(), positions.Skip(16).Take(4).ToArray());
        }

        [Test]
        public void Extend_ClearanceBelow_LimitsHowFarThePartReaches()
        {
            var positions = CreateArcPart(0.8f, 8);
            var original = positions.ToArray();
            var unrestricted = original.ToArray();

            CurveExtendOperation.Extend(unrestricted, 2f, 0.2f, 0f, 0f);
            CurveExtendOperation.Extend(positions, 2f, 0.2f, 0f, 0.3f);

            var limit = original.Min(position => position.y);

            Assert.Less(unrestricted.Min(position => position.y), -0.3f);
            Assert.GreaterOrEqual(positions.Min(position => position.y), -0.3f - Tolerance);
            Assert.Less(positions.Min(position => position.y), limit - 0.005f);
        }

        [Test]
        public void Extend_PartAboveTheExitOnly_DoesNothing()
        {
            var positions = new[] { new Vector3(0f, 0.1f, 0f), new Vector3(0.1f, 0.2f, 0.1f), Vector3.up, Vector3.one };
            var original = positions.ToArray();

            CurveExtendOperation.Extend(positions, 2f, 0.2f, 0f, 0f);

            CollectionAssert.AreEqual(original, positions);
        }

        [Test]
        public void Extend_AnyPart_NeverProducesInvalidNumbers()
        {
            var parts = new[] { CreateArcPart(0.8f, 8), CreateStraightPart(0f), CreateStraightPart(0.4f), CreateArcPart(0.05f, 3) };

            foreach (var part in parts)
            {
                CurveExtendOperation.Extend(part, 3f, 0.9f, 0.01f, 0.5f);

                foreach (var position in part)
                {
                    Assert.IsFalse(float.IsNaN(position.x) || float.IsNaN(position.y) || float.IsNaN(position.z));
                    Assert.IsFalse(float.IsInfinity(position.x) || float.IsInfinity(position.y) || float.IsInfinity(position.z));
                }
            }
        }

        [Test]
        public void RotateDirections_FollowsTheFacesOfTheirVertices()
        {
            var oldPositions = new[] { new Vector3(0f, 0f, 0f), new Vector3(1f, 0f, 0f), new Vector3(0f, 1f, 0f) };
            var newPositions = oldPositions.Select(position => new Vector3(position.x, -position.z, position.y)).ToArray();
            var normals = new[] { Vector3.forward, Vector3.forward, Vector3.forward };

            WeaponPartMeshFactory.RotateDirections(oldPositions, newPositions, new[] { 0, 1, 2 }, normals);

            foreach (var normal in normals)
                AssertVector(new Vector3(0f, -1f, 0f), normal);
        }

        [Test]
        public void RotateDirections_UnchangedFaces_KeepTheDirections()
        {
            var positions = new[] { Vector3.zero, Vector3.right, Vector3.up };
            var directions = new[] { new Vector3(0.6f, 0f, 0.8f), Vector3.forward, Vector3.back };

            WeaponPartMeshFactory.RotateDirections(positions, positions.ToArray(), new[] { 0, 1, 2 }, directions);

            AssertVector(new Vector3(0.6f, 0f, 0.8f), directions[0]);
            AssertVector(Vector3.forward, directions[1]);
            AssertVector(Vector3.back, directions[2]);
        }

        [Test, Category("Mesh")]
        public void Apply_RadialThicken_KeepsTopologyUvsAndSkinning()
        {
            var source = CreateTube(6, 12, 0.4f, 0.03f, Vector3.zero);
            var operation = CreateOperation<RadialThickenOperation>(("radialScale", 1.4f), ("minRadius", 0f));
            var context = new WeaponPartContext(source, Matrix4x4.identity, 0f, 0f);

            var result = operation.Apply(in context);

            AssertTopologyPreserved(source, result);
            Assert.AreEqual(0.03f * 1.4f, Mathf.Sqrt(result.vertices[0].x * result.vertices[0].x + result.vertices[0].y * result.vertices[0].y), Tolerance);
            Assert.AreNotSame(source, result);
        }

        [Test, Category("Mesh")]
        public void Apply_RadialThicken_MeasuresFromTheMountAxis()
        {
            var offset = new Vector3(0.2f, 0.1f, 0f);
            var source = CreateTube(4, 8, 0.4f, 0.03f, offset);
            var meshToMount = Matrix4x4.Translate(-offset);
            var operation = CreateOperation<RadialThickenOperation>(("radialScale", 2f), ("minRadius", 0f));
            var context = new WeaponPartContext(source, meshToMount, 0f, 0f);

            var result = operation.Apply(in context);

            foreach (var vertex in result.vertices)
            {
                var fromAxis = new Vector2(vertex.x - offset.x, vertex.y - offset.y).magnitude;

                Assert.AreEqual(0.06f, fromAxis, Tolerance);
            }
        }

        [Test, Category("Mesh")]
        public void Apply_RadialThicken_KeepsFlatShadingFlat()
        {
            var source = CreateBox(new Vector3(0.05f, 0.02f, 0.2f));
            var operation = CreateOperation<RadialThickenOperation>(("radialScale", 1.5f), ("minRadius", 0f));
            var context = new WeaponPartContext(source, Matrix4x4.identity, 0f, 0f);

            var result = operation.Apply(in context);

            var sourceNormals = source.normals;
            var resultNormals = result.normals;

            for (var i = 0; i < sourceNormals.Length; i++)
                AssertVector(sourceNormals[i], resultNormals[i]);
        }

        [Test, Category("Mesh")]
        public void Apply_CurveExtend_KeepsTopologyUvsAndSkinning()
        {
            var source = CreateMeshFromPositions(CreateArcPart(0.8f, 8));
            var operation = CreateOperation<CurveExtendOperation>(("lengthScale", 1.4f), ("rigidEndFraction", 0.2f));
            var context = new WeaponPartContext(source, Matrix4x4.identity, 0f, 0f);

            var result = operation.Apply(in context);

            AssertTopologyPreserved(source, result);
            Assert.Less(result.bounds.min.y, source.bounds.min.y);
        }

        [Test, Category("Mesh")]
        public void Apply_WithoutSourceMesh_Throws()
        {
            var operation = CreateOperation<RadialThickenOperation>(("radialScale", 1.2f));
            var context = new WeaponPartContext(null, Matrix4x4.identity, 0f, 0f);

            Assert.Throws<ArgumentNullException>(() => operation.Apply(in context));
        }

        [Test, Category("Mesh")]
        public void Cache_SameSourceOperationAndPlacement_CreatesTheMeshOnce()
        {
            var source = CreateTube(4, 8, 0.4f, 0.03f, Vector3.zero);
            var operation = CreateOperation<RadialThickenOperation>(("radialScale", 1.3f));
            var context = new WeaponPartContext(source, Matrix4x4.identity, 0f, 0f);
            var moved = new WeaponPartContext(source, Matrix4x4.Translate(Vector3.one), 0f, 0f);

            var first = WeaponPartMeshCache.GetOrCreate(operation, in context);
            var second = WeaponPartMeshCache.GetOrCreate(operation, in context);
            var other = WeaponPartMeshCache.GetOrCreate(operation, in moved);

            Assert.AreSame(first, second);
            Assert.AreNotSame(first, other);
            Assert.AreEqual(2, WeaponPartMeshCache.Count);
        }

        [Test, Category("Mesh")]
        public void Cache_InvalidatedOperation_CreatesTheMeshAgain()
        {
            var source = CreateTube(4, 8, 0.4f, 0.03f, Vector3.zero);
            var operation = CreateOperation<RadialThickenOperation>(("radialScale", 1.3f));
            var context = new WeaponPartContext(source, Matrix4x4.identity, 0f, 0f);

            var first = WeaponPartMeshCache.GetOrCreate(operation, in context);

            WeaponPartMeshCache.Invalidate(operation);

            var second = WeaponPartMeshCache.GetOrCreate(operation, in context);

            Assert.AreNotSame(first, second);
            Assert.AreEqual(1, WeaponPartMeshCache.Count);
        }

        private static void AssertVector(Vector3 expected, Vector3 actual)
        {
            Assert.AreEqual(expected.x, actual.x, Tolerance);
            Assert.AreEqual(expected.y, actual.y, Tolerance);
            Assert.AreEqual(expected.z, actual.z, Tolerance);
        }

        private static Vector2 GetArcPoint(float angle) =>
            new(ArcRadius - ArcRadius * Mathf.Cos(angle), ArcRadius * Mathf.Sin(angle));

        private static Vector3[] CreateArcPart(float maxAngle, int slices)
        {
            var positions = new List<Vector3>();

            for (var slice = -1; slice <= slices; slice++)
            {
                var angle = maxAngle * slice / slices;

                foreach (var width in new[] { -ArcHalfWidth, ArcHalfWidth })
                {
                    foreach (var thickness in new[] { -ArcHalfThickness, ArcHalfThickness })
                    {
                        var radius = ArcRadius + thickness;
                        var z = ArcRadius - radius * Mathf.Cos(angle);
                        var u = radius * Mathf.Sin(angle);

                        positions.Add(new Vector3(width, -u, z));
                    }
                }
            }

            return positions.ToArray();
        }

        private static Vector3[] CreateStraightPart(float tilt)
        {
            var positions = new List<Vector3>();

            for (var ring = -1; ring <= 4; ring++)
            {
                var depth = ring * 0.05f;

                foreach (var width in new[] { -0.02f, 0.02f })
                {
                    foreach (var thickness in new[] { -0.01f, 0.01f })
                        positions.Add(new Vector3(width, -depth, thickness + tilt * depth));
                }
            }

            return positions.ToArray();
        }

        private static Vector3[] GetSlice(Vector3[] positions, int slice)
        {
            var start = (slice + 1) * 4;

            return positions.Skip(start).Take(4).ToArray();
        }

        private static Vector3 GetSliceCenter(Vector3[] positions, int slice)
        {
            var center = Vector3.zero;

            foreach (var position in GetSlice(positions, slice))
                center += position;

            return center / 4f;
        }

        private static void AssertSameCrossSection(Vector3[] before, Vector3[] after)
        {
            for (var i = 0; i < before.Length; i++)
            {
                Assert.AreEqual(before[i].x, after[i].x, Tolerance);

                for (var j = i + 1; j < before.Length; j++)
                {
                    Assert.AreEqual(
                        Vector3.Distance(before[i], before[j]),
                        Vector3.Distance(after[i], after[j]),
                        ArcTolerance);
                }
            }
        }

        private T CreateOperation<T>(params (string Field, float Value)[] values) where T : WeaponPartOperation
        {
            var operation = ScriptableObject.CreateInstance<T>();
            var serialized = new SerializedObject(operation);

            foreach (var (field, value) in values)
                serialized.FindProperty(field).floatValue = value;

            serialized.ApplyModifiedPropertiesWithoutUndo();
            _created.Add(operation);

            return operation;
        }

        private Mesh CreateTube(int rings, int segments, float length, float radius, Vector3 axisOffset)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var tangents = new List<Vector4>();
            var uvs = new List<Vector2>();
            var weights = new List<BoneWeight>();

            for (var ring = 0; ring < rings; ring++)
            {
                for (var segment = 0; segment < segments; segment++)
                {
                    var angle = 2f * Mathf.PI * segment / segments;
                    var direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);

                    vertices.Add(axisOffset + direction * radius + Vector3.forward * (length * ring / (rings - 1)));
                    normals.Add(direction);
                    tangents.Add(new Vector4(-direction.y, direction.x, 0f, 1f));
                    uvs.Add(new Vector2((float)segment / segments, (float)ring / (rings - 1)));
                    weights.Add(new BoneWeight { boneIndex0 = segment % 2, weight0 = 0.75f, boneIndex1 = 1 - segment % 2, weight1 = 0.25f });
                }
            }

            var firstBand = new List<int>();
            var secondBand = new List<int>();

            for (var ring = 0; ring < rings - 1; ring++)
            {
                var target = ring < (rings - 1) / 2 ? firstBand : secondBand;

                for (var segment = 0; segment < segments; segment++)
                {
                    var next = (segment + 1) % segments;
                    var a = ring * segments + segment;
                    var b = ring * segments + next;
                    var c = (ring + 1) * segments + segment;
                    var d = (ring + 1) * segments + next;

                    target.AddRange(new[] { a, c, b, b, c, d });
                }
            }

            var mesh = new Mesh { name = "Tube" };

            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTangents(tangents);
            mesh.SetUVs(0, uvs);
            mesh.boneWeights = weights.ToArray();
            mesh.bindposes = new[] { Matrix4x4.identity, Matrix4x4.Translate(Vector3.up) };
            mesh.subMeshCount = 2;
            mesh.SetTriangles(firstBand, 0);
            mesh.SetTriangles(secondBand, 1);
            mesh.RecalculateBounds();
            _created.Add(mesh);

            return mesh;
        }

        private Mesh CreateBox(Vector3 halfSize)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var triangles = new List<int>();
            var directions = new[] { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back };

            foreach (var normal in directions)
            {
                var tangent = Vector3.Cross(normal, Mathf.Abs(normal.y) > 0.5f ? Vector3.forward : Vector3.up).normalized;
                var bitangent = Vector3.Cross(normal, tangent);
                var start = vertices.Count;

                foreach (var corner in new[] { (-1f, -1f), (1f, -1f), (1f, 1f), (-1f, 1f) })
                {
                    var local = normal + tangent * corner.Item1 + bitangent * corner.Item2;

                    vertices.Add(Vector3.Scale(local, halfSize));
                    normals.Add(normal);
                }

                triangles.AddRange(new[] { start, start + 1, start + 2, start, start + 2, start + 3 });
            }

            var mesh = new Mesh { name = "Box" };

            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            _created.Add(mesh);

            return mesh;
        }

        private Mesh CreateMeshFromPositions(Vector3[] positions)
        {
            var triangles = new List<int>();

            for (var slice = 0; slice < positions.Length / 4 - 1; slice++)
            {
                var a = slice * 4;
                var b = a + 4;

                triangles.AddRange(new[] { a, b, a + 1, a + 1, b, b + 1, a + 2, a + 3, b + 2, a + 3, b + 3, b + 2 });
            }

            var mesh = new Mesh { name = "Arc" };

            mesh.SetVertices(positions);
            mesh.SetUVs(0, positions.Select(position => new Vector2(position.x, position.z)).ToArray());
            mesh.boneWeights = positions.Select(_ => new BoneWeight { boneIndex0 = 0, weight0 = 1f }).ToArray();
            mesh.bindposes = new[] { Matrix4x4.identity };
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            _created.Add(mesh);

            return mesh;
        }

        private void AssertTopologyPreserved(Mesh source, Mesh result)
        {
            _created.Add(result);

            Assert.AreEqual(source.vertexCount, result.vertexCount);
            Assert.AreEqual(source.subMeshCount, result.subMeshCount);

            for (var i = 0; i < source.subMeshCount; i++)
                CollectionAssert.AreEqual(source.GetTriangles(i), result.GetTriangles(i));

            CollectionAssert.AreEqual(source.uv, result.uv);
            CollectionAssert.AreEqual(source.bindposes, result.bindposes);

            var sourceWeights = source.boneWeights;
            var resultWeights = result.boneWeights;

            Assert.AreEqual(sourceWeights.Length, resultWeights.Length);

            for (var i = 0; i < sourceWeights.Length; i++)
                Assert.AreEqual(sourceWeights[i], resultWeights[i]);

            foreach (var normal in result.normals)
            {
                Assert.IsFalse(float.IsNaN(normal.x) || float.IsNaN(normal.y) || float.IsNaN(normal.z));
                Assert.AreEqual(1f, normal.magnitude, 1e-3f);
            }

            var bounds = result.bounds;

            bounds.Expand(1e-4f);

            foreach (var vertex in result.vertices)
            {
                Assert.IsFalse(float.IsNaN(vertex.x) || float.IsNaN(vertex.y) || float.IsNaN(vertex.z));
                Assert.IsTrue(bounds.Contains(vertex));
            }
        }
    }
}
