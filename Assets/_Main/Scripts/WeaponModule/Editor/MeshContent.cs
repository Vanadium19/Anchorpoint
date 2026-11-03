using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace WeaponModule
{
    /// <summary>
    /// A managed copy of everything <see cref="WeaponMeshPartExtractor"/> keeps from a mesh. Geometry is read through
    /// <see cref="Mesh.AcquireReadOnlyMeshData(Mesh)"/>, so the mesh does not need Read/Write enabled and no import
    /// setting is touched.
    /// </summary>
    internal sealed class MeshContent
    {
        private const int UvChannelCount = 8;

        private readonly string _name;
        private readonly Vector3[] _positions;
        private readonly Vector3[] _normals;
        private readonly Vector4[] _tangents;
        private readonly Color[] _colors;
        private readonly Vector4[][] _uvs = new Vector4[UvChannelCount][];
        private readonly int[] _uvDimensions = new int[UvChannelCount];
        private readonly int[] _indices;
        private readonly int[] _submeshTriangleStarts;
        private readonly Matrix4x4[] _bindposes;
        private readonly byte[] _bonesPerVertex;
        private readonly BoneWeight1[] _boneWeights;
        private readonly int[] _boneWeightStarts;

        public MeshContent(Mesh source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (source.blendShapeCount > 0)
                throw new NotSupportedException($"Mesh '{source.name}' has blend shapes, which are not supported.");

            _name = source.name;

            using var dataArray = Mesh.AcquireReadOnlyMeshData(source);
            var data = dataArray[0];

            VertexCount = data.vertexCount;
            SubmeshCount = data.subMeshCount;
            _positions = Read<Vector3>(VertexCount, native => data.GetVertices(native));

            if (data.HasVertexAttribute(VertexAttribute.Normal))
                _normals = Read<Vector3>(VertexCount, native => data.GetNormals(native));

            if (data.HasVertexAttribute(VertexAttribute.Tangent))
                _tangents = Read<Vector4>(VertexCount, native => data.GetTangents(native));

            if (data.HasVertexAttribute(VertexAttribute.Color))
                _colors = Read<Color>(VertexCount, native => data.GetColors(native));

            for (var channel = 0; channel < UvChannelCount; channel++)
                ReadUvChannel(data, channel);

            _submeshTriangleStarts = new int[SubmeshCount + 1];
            _indices = ReadIndices(data);

            if (TryReadSkin(source, out _bindposes, out _bonesPerVertex, out _boneWeights))
                _boneWeightStarts = BuildBoneWeightStarts(_bonesPerVertex);
        }

        public int VertexCount { get; }
        public int SubmeshCount { get; }
        public int TriangleCount => _indices.Length / 3;
        public bool HasSkin => _bonesPerVertex != null;

        public Vector3 GetPosition(int vertex) => _positions[vertex];

        public int GetTriangleVertex(int triangle, int corner) => _indices[triangle * 3 + corner];

        public int GetSubmesh(int triangle)
        {
            for (var submesh = 0; submesh < SubmeshCount; submesh++)
            {
                if (triangle < _submeshTriangleStarts[submesh + 1])
                    return submesh;
            }

            throw new ArgumentOutOfRangeException(nameof(triangle));
        }

        public MeshTriangle GetTriangle(int triangle) => new(
            triangle,
            GetSubmesh(triangle),
            _positions[GetTriangleVertex(triangle, 0)],
            _positions[GetTriangleVertex(triangle, 1)],
            _positions[GetTriangleVertex(triangle, 2)]);

        public void AddBoneWeights(int vertex, Dictionary<int, float> weightByBone)
        {
            var start = _boneWeightStarts[vertex];

            for (var i = 0; i < _bonesPerVertex[vertex]; i++)
            {
                var weight = _boneWeights[start + i];

                weightByBone.TryGetValue(weight.boneIndex, out var sum);
                weightByBone[weight.boneIndex] = sum + weight.weight;
            }
        }

        public Mesh Build(string name, bool[] isSelected, bool wantedSelection)
        {
            var sourceVertices = CollectVertices(isSelected, wantedSelection, out var remap);
            var mesh = new Mesh
            {
                name = name,
                indexFormat = sourceVertices.Count > ushort.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16
            };

            mesh.SetVertices(Gather(_positions, sourceVertices));

            if (_normals != null)
                mesh.SetNormals(Gather(_normals, sourceVertices));

            if (_tangents != null)
                mesh.SetTangents(Gather(_tangents, sourceVertices));

            if (_colors != null)
                mesh.SetColors(Gather(_colors, sourceVertices));

            for (var channel = 0; channel < UvChannelCount; channel++)
                SetUvChannel(mesh, channel, sourceVertices);

            mesh.subMeshCount = SubmeshCount;

            for (var submesh = 0; submesh < SubmeshCount; submesh++)
                mesh.SetTriangles(CollectTriangles(submesh, isSelected, wantedSelection, remap), submesh, false);

            if (HasSkin)
                SetSkin(mesh, sourceVertices);

            mesh.RecalculateBounds();

            return mesh;
        }

        private static T[] Read<T>(int count, Action<NativeArray<T>> read) where T : struct
        {
            var native = new NativeArray<T>(count, Allocator.Temp);

            read(native);

            var result = native.ToArray();

            native.Dispose();

            return result;
        }

        private static List<T> Gather<T>(T[] values, List<int> sourceVertices)
        {
            var result = new List<T>(sourceVertices.Count);

            foreach (var vertex in sourceVertices)
                result.Add(values[vertex]);

            return result;
        }

        private static bool TryReadSkin(Mesh source, out Matrix4x4[] bindposes, out byte[] bonesPerVertex,
            out BoneWeight1[] boneWeights)
        {
            var sourceBonesPerVertex = source.GetBonesPerVertex();

            bindposes = null;
            bonesPerVertex = null;
            boneWeights = null;

            if (!sourceBonesPerVertex.IsCreated || sourceBonesPerVertex.Length != source.vertexCount ||
                source.bindposeCount == 0)
            {
                return false;
            }

            bindposes = source.bindposes;
            bonesPerVertex = sourceBonesPerVertex.ToArray();
            boneWeights = source.GetAllBoneWeights().ToArray();

            return true;
        }

        private static int[] BuildBoneWeightStarts(byte[] bonesPerVertex)
        {
            var starts = new int[bonesPerVertex.Length];
            var next = 0;

            for (var vertex = 0; vertex < bonesPerVertex.Length; vertex++)
            {
                starts[vertex] = next;
                next += bonesPerVertex[vertex];
            }

            return starts;
        }

        private void ReadUvChannel(Mesh.MeshData data, int channel)
        {
            var attribute = VertexAttribute.TexCoord0 + channel;

            if (!data.HasVertexAttribute(attribute))
                return;

            var dimension = data.GetVertexAttributeDimension(attribute);
            var values = new Vector4[VertexCount];

            _uvDimensions[channel] = dimension;
            _uvs[channel] = values;

            if (dimension == 2)
            {
                var uvs = Read<Vector2>(VertexCount, native => data.GetUVs(channel, native));

                for (var i = 0; i < values.Length; i++)
                    values[i] = uvs[i];
            }
            else if (dimension == 3)
            {
                var uvs = Read<Vector3>(VertexCount, native => data.GetUVs(channel, native));

                for (var i = 0; i < values.Length; i++)
                    values[i] = uvs[i];
            }
            else
            {
                values = Read<Vector4>(VertexCount, native => data.GetUVs(channel, native));
                _uvs[channel] = values;
            }
        }

        private int[] ReadIndices(Mesh.MeshData data)
        {
            var indices = new List<int>();

            for (var submesh = 0; submesh < SubmeshCount; submesh++)
            {
                var descriptor = data.GetSubMesh(submesh);

                if (descriptor.topology != MeshTopology.Triangles)
                {
                    throw new NotSupportedException(
                        $"Submesh {submesh} of mesh '{_name}' is {descriptor.topology}; only triangles are supported.");
                }

                var native = new NativeArray<int>(descriptor.indexCount, Allocator.Temp);

                data.GetIndices(native, submesh);
                indices.AddRange(native.ToArray());
                native.Dispose();

                _submeshTriangleStarts[submesh + 1] = indices.Count / 3;
            }

            return indices.ToArray();
        }

        private List<int> CollectVertices(bool[] isSelected, bool wantedSelection, out int[] remap)
        {
            var isUsed = new bool[VertexCount];

            for (var triangle = 0; triangle < TriangleCount; triangle++)
            {
                if (isSelected[triangle] != wantedSelection)
                    continue;

                for (var corner = 0; corner < 3; corner++)
                    isUsed[GetTriangleVertex(triangle, corner)] = true;
            }

            var sourceVertices = new List<int>();

            remap = new int[VertexCount];

            for (var vertex = 0; vertex < VertexCount; vertex++)
            {
                remap[vertex] = -1;

                if (!isUsed[vertex])
                    continue;

                remap[vertex] = sourceVertices.Count;
                sourceVertices.Add(vertex);
            }

            return sourceVertices;
        }

        private List<int> CollectTriangles(int submesh, bool[] isSelected, bool wantedSelection, int[] remap)
        {
            var result = new List<int>();

            for (var triangle = _submeshTriangleStarts[submesh]; triangle < _submeshTriangleStarts[submesh + 1];
                 triangle++)
            {
                if (isSelected[triangle] != wantedSelection)
                    continue;

                for (var corner = 0; corner < 3; corner++)
                    result.Add(remap[GetTriangleVertex(triangle, corner)]);
            }

            return result;
        }

        private void SetUvChannel(Mesh mesh, int channel, List<int> sourceVertices)
        {
            var values = _uvs[channel];

            if (values == null)
                return;

            var gathered = Gather(values, sourceVertices);

            if (_uvDimensions[channel] == 2)
                mesh.SetUVs(channel, gathered.ConvertAll(value => (Vector2)value));
            else if (_uvDimensions[channel] == 3)
                mesh.SetUVs(channel, gathered.ConvertAll(value => (Vector3)value));
            else
                mesh.SetUVs(channel, gathered);
        }

        private void SetSkin(Mesh mesh, List<int> sourceVertices)
        {
            var bonesPerVertex = new byte[sourceVertices.Count];
            var boneWeights = new List<BoneWeight1>();

            for (var i = 0; i < sourceVertices.Count; i++)
            {
                var vertex = sourceVertices[i];

                bonesPerVertex[i] = _bonesPerVertex[vertex];

                for (var weight = 0; weight < _bonesPerVertex[vertex]; weight++)
                    boneWeights.Add(_boneWeights[_boneWeightStarts[vertex] + weight]);
            }

            var nativeBonesPerVertex = new NativeArray<byte>(bonesPerVertex, Allocator.Temp);
            var nativeBoneWeights = new NativeArray<BoneWeight1>(boneWeights.ToArray(), Allocator.Temp);

            mesh.bindposes = _bindposes;
            mesh.SetBoneWeights(nativeBonesPerVertex, nativeBoneWeights);

            nativeBonesPerVertex.Dispose();
            nativeBoneWeights.Dispose();
        }
    }
}
