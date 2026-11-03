using System;
using System.Collections.Generic;
using UnityEngine;

namespace WeaponModule
{
    /// <summary>
    /// Makes a part longer along its own curve, for example a curved magazine. The mount origin lies at the exit
    /// of the part (the well of the magazine), mount -Y leads into the length of the part and +Z is forward.
    /// A circular arc is fitted to the vertices below the exit. The arc length between the exit and the rigid end
    /// is stretched, the cross-section stays as it is and the rigid end moves along the arc as one piece. Nothing
    /// above the exit moves. A part without a curve is extended in a straight line.
    /// </summary>
    [CreateAssetMenu(fileName = "CurveExtendOperation", menuName = "Configs/Weapon Part Operations/Curve Extend")]
    public class CurveExtendOperation : WeaponPartOperation
    {
        private const float ExitTolerance = 1e-6f;
        private const float MinExtent = 1e-6f;
        private const float EndShare = 0.15f;
        private const float MinArcAngle = 0.02f;
        private const float MaxArcAngle = 2.5f;
        private const float MinDownwardComponent = 0.2f;
        private const float MaxRigidEndFraction = 0.9f;
        private const float MaxFitErrorRatio = 0.2f;
        private const float MaxEdgeSteps = 64f;
        private const int SamplesPerRange = 64;
        private const int MaxFitSamples = 3000;
        private const int MinPartVertices = 4;
        private const int ClearanceSearchSteps = 20;

        [Tooltip("Length of the part below the exit after the operation, as a multiple of its original length.")]
        [Min(1f)]
        [SerializeField] private float lengthScale = 1.3f;
        [Tooltip("Share of the original length at the far end that moves as one piece and is not stretched.")]
        [Range(0f, MaxRigidEndFraction)]
        [SerializeField] private float rigidEndFraction = 0.15f;

        public float LengthScale => lengthScale;
        public float RigidEndFraction => rigidEndFraction;

        /// <summary>
        /// Extends the part below the exit plane. The positions are in the space of the mount and are changed in
        /// place. A part that has no vertices below the exit, or is too small to measure, is left unchanged.
        /// </summary>
        /// <param name="positions">Vertex positions in the space of the mount.</param>
        /// <param name="lengthScale">Length after the operation as a multiple of the length before. Below 1 does nothing.</param>
        /// <param name="rigidEndFraction">Share of the length at the far end that moves as one piece.</param>
        /// <param name="clearanceAbove">Room above the axis in metres, 0 for unlimited.</param>
        /// <param name="clearanceBelow">Room below the axis in metres, 0 for unlimited.</param>
        public static void Extend(
            Vector3[] positions,
            float lengthScale,
            float rigidEndFraction,
            float clearanceAbove,
            float clearanceBelow) =>
            Extend(positions, null, lengthScale, rigidEndFraction, clearanceAbove, clearanceBelow);

        /// <summary>
        /// Extends the part below the exit plane. The triangles let the curve fit see the surface of the part
        /// between its vertices, which a low poly part needs. Otherwise like the overload without triangles.
        /// </summary>
        /// <param name="positions">Vertex positions in the space of the mount.</param>
        /// <param name="triangles">Triangle indices into the positions, three per triangle. Optional.</param>
        /// <param name="lengthScale">Length after the operation as a multiple of the length before. Below 1 does nothing.</param>
        /// <param name="rigidEndFraction">Share of the length at the far end that moves as one piece.</param>
        /// <param name="clearanceAbove">Room above the axis in metres, 0 for unlimited.</param>
        /// <param name="clearanceBelow">Room below the axis in metres, 0 for unlimited.</param>
        public static void Extend(
            Vector3[] positions,
            int[] triangles,
            float lengthScale,
            float rigidEndFraction,
            float clearanceAbove,
            float clearanceBelow)
        {
            if (positions == null)
                throw new ArgumentNullException(nameof(positions));

            if (float.IsNaN(lengthScale) || float.IsInfinity(lengthScale))
                throw new ArgumentOutOfRangeException(nameof(lengthScale), lengthScale, "The scale must be finite.");

            var extra = lengthScale - 1f;
            var indices = FindPartIndices(positions);

            if (extra <= 0f || indices.Length < MinPartVertices)
                return;

            var rigid = Mathf.Clamp(rigidEndFraction, 0f, MaxRigidEndFraction);
            var samples = CollectSamples(positions, indices, triangles);
            var extension = Extension.Create(positions, indices, samples, rigid);

            if (extension == null)
                return;

            extension.Apply(FindAllowedExtra(positions, extension, extra, clearanceAbove, clearanceBelow));
        }

        /// <inheritdoc/>
        public override Mesh Apply(in WeaponPartContext context)
        {
            var positions = WeaponPartMeshFactory.ReadPositions(in context);

            var triangles = WeaponPartMeshFactory.ReadTriangles(in context);

            Extend(positions, triangles, lengthScale, rigidEndFraction, context.ClearanceAbove, context.ClearanceBelow);

            return WeaponPartMeshFactory.Create(in context, positions, $"{context.Source.name}_{name}");
        }

        private static int[] FindPartIndices(Vector3[] positions)
        {
            var count = 0;

            for (var i = 0; i < positions.Length; i++)
            {
                if (-positions[i].y > ExitTolerance)
                    count++;
            }

            var indices = new int[count];
            var next = 0;

            for (var i = 0; i < positions.Length; i++)
            {
                if (-positions[i].y > ExitTolerance)
                    indices[next++] = i;
            }

            return indices;
        }

        private static Vector3[] CollectSamples(Vector3[] positions, int[] indices, int[] triangles)
        {
            var samples = new List<Vector3>(indices.Length);
            var minU = float.MaxValue;
            var maxU = float.MinValue;

            for (var k = 0; k < indices.Length; k++)
            {
                samples.Add(positions[indices[k]]);
                minU = Mathf.Min(minU, -positions[indices[k]].y);
                maxU = Mathf.Max(maxU, -positions[indices[k]].y);
            }

            if (triangles == null || maxU - minU < MinExtent)
                return Thin(samples);

            var spacing = (maxU - minU) / SamplesPerRange;

            for (var i = 0; i + 2 < triangles.Length; i += 3)
            {
                for (var edge = 0; edge < 3; edge++)
                {
                    var from = positions[triangles[i + edge]];
                    var to = positions[triangles[i + (edge + 1) % 3]];
                    var steps = Mathf.Min(MaxEdgeSteps, Mathf.Floor((to - from).magnitude / spacing));

                    for (var step = 1; step < steps; step++)
                    {
                        var sample = Vector3.Lerp(from, to, step / steps);

                        if (-sample.y > ExitTolerance)
                            samples.Add(sample);
                    }
                }
            }

            return Thin(samples);
        }

        private static Vector3[] Thin(List<Vector3> samples)
        {
            var stride = Mathf.CeilToInt((float)samples.Count / MaxFitSamples);

            if (stride <= 1)
                return samples.ToArray();

            var thinned = new Vector3[(samples.Count + stride - 1) / stride];

            for (var i = 0; i < thinned.Length; i++)
                thinned[i] = samples[i * stride];

            return thinned;
        }

        private static float FindAllowedExtra(
            Vector3[] positions,
            Extension extension,
            float extra,
            float clearanceAbove,
            float clearanceBelow)
        {
            if (clearanceAbove <= 0f && clearanceBelow <= 0f)
                return extra;

            var minY = float.MaxValue;
            var maxY = float.MinValue;

            for (var i = 0; i < positions.Length; i++)
            {
                minY = Mathf.Min(minY, positions[i].y);
                maxY = Mathf.Max(maxY, positions[i].y);
            }

            var allowedMinY = clearanceBelow > 0f ? Mathf.Min(minY, -clearanceBelow) : float.MinValue;
            var allowedMaxY = clearanceAbove > 0f ? Mathf.Max(maxY, clearanceAbove) : float.MaxValue;

            if (extension.Fits(extra, allowedMinY, allowedMaxY))
                return extra;

            var low = 0f;
            var high = 1f;

            for (var step = 0; step < ClearanceSearchSteps; step++)
            {
                var middle = (low + high) * 0.5f;

                if (extension.Fits(extra * middle, allowedMinY, allowedMaxY))
                    low = middle;
                else
                    high = middle;
            }

            return extra * low;
        }

        private sealed class Extension
        {
            private readonly Vector3[] _positions;
            private readonly int[] _indices;
            private readonly float[] _parameters;
            private readonly float[] _angles;
            private readonly float[] _radii;
            private readonly bool _isArc;
            private readonly float _centerZ;
            private readonly float _centerU;
            private readonly float _sign;
            private readonly float _directionZ;
            private readonly float _directionU;
            private readonly float _maxParameter;
            private readonly float _rigidStart;

            private Extension(
                Vector3[] positions,
                int[] indices,
                float[] parameters,
                float rigidEndFraction,
                float[] angles,
                float[] radii,
                float centerZ,
                float centerU,
                float sign,
                float directionZ,
                float directionU)
            {
                _positions = positions;
                _indices = indices;
                _parameters = parameters;
                _angles = angles;
                _radii = radii;
                _isArc = angles != null;
                _centerZ = centerZ;
                _centerU = centerU;
                _sign = sign;
                _directionZ = directionZ;
                _directionU = directionU;

                for (var i = 0; i < parameters.Length; i++)
                    _maxParameter = Mathf.Max(_maxParameter, parameters[i]);

                _rigidStart = (1f - rigidEndFraction) * _maxParameter;
            }

            public static Extension Create(Vector3[] positions, int[] indices, Vector3[] samples, float rigidEndFraction)
            {
                var minU = float.MaxValue;
                var maxU = float.MinValue;

                for (var i = 0; i < samples.Length; i++)
                {
                    var u = -samples[i].y;

                    minU = Mathf.Min(minU, u);
                    maxU = Mathf.Max(maxU, u);
                }

                var range = maxU - minU;

                if (range < MinExtent)
                    return null;

                var nearLimit = minU + range * EndShare;
                var farLimit = maxU - range * EndShare;

                return CreateArc(positions, indices, samples, rigidEndFraction, nearLimit, farLimit)
                    ?? CreateStraight(positions, indices, samples, rigidEndFraction, nearLimit, farLimit);
            }

            public bool Fits(float extra, float allowedMinY, float allowedMaxY)
            {
                for (var k = 0; k < _indices.Length; k++)
                {
                    var y = Move(k, extra).y;

                    if (y < allowedMinY || y > allowedMaxY)
                        return false;
                }

                return true;
            }

            public void Apply(float extra)
            {
                for (var k = 0; k < _indices.Length; k++)
                    _positions[_indices[k]] = Move(k, extra);
            }

            private static Extension CreateArc(
                Vector3[] positions,
                int[] indices,
                Vector3[] samples,
                float rigidEndFraction,
                float nearLimit,
                float farLimit)
            {
                if (!WeaponPartCircleFit.TryFit(samples, out var centerZ, out var centerU))
                    return null;

                var sampleRadii = new float[samples.Length];
                var nearSin = 0f;
                var nearCos = 0f;
                var farSin = 0f;
                var farCos = 0f;

                for (var k = 0; k < samples.Length; k++)
                {
                    var offsetZ = samples[k].z - centerZ;
                    var offsetU = -samples[k].y - centerU;
                    var angle = Mathf.Atan2(offsetU, offsetZ);

                    sampleRadii[k] = Mathf.Sqrt(offsetZ * offsetZ + offsetU * offsetU);

                    if (-samples[k].y <= nearLimit)
                    {
                        nearSin += Mathf.Sin(angle);
                        nearCos += Mathf.Cos(angle);
                    }

                    if (-samples[k].y >= farLimit)
                    {
                        farSin += Mathf.Sin(angle);
                        farCos += Mathf.Cos(angle);
                    }
                }

                if (!IsCircular(sampleRadii))
                    return null;

                var count = indices.Length;
                var angles = new float[count];
                var radii = new float[count];

                for (var k = 0; k < count; k++)
                {
                    var position = positions[indices[k]];
                    var offsetZ = position.z - centerZ;
                    var offsetU = -position.y - centerU;

                    angles[k] = Mathf.Atan2(offsetU, offsetZ);
                    radii[k] = Mathf.Sqrt(offsetZ * offsetZ + offsetU * offsetU);
                }

                var nearAngle = Mathf.Atan2(nearSin, nearCos);
                var arcAngle = WrapAngle(Mathf.Atan2(farSin, farCos) - nearAngle);
                var sign = arcAngle >= 0f ? 1f : -1f;

                if (Mathf.Abs(arcAngle) < MinArcAngle || Mathf.Abs(arcAngle) > MaxArcAngle)
                    return null;

                var parameters = new float[count];

                for (var k = 0; k < count; k++)
                {
                    var exitAngle = FindExitAngle(radii[k], centerU, nearAngle);

                    parameters[k] = Mathf.Max(0f, sign * WrapAngle(angles[k] - exitAngle));
                }

                return new Extension(
                    positions, indices, parameters, rigidEndFraction, angles, radii, centerZ, centerU, sign, 0f, 0f);
            }

            private static Extension CreateStraight(
                Vector3[] positions,
                int[] indices,
                Vector3[] samples,
                float rigidEndFraction,
                float nearLimit,
                float farLimit)
            {
                var nearZ = 0f;
                var nearU = 0f;
                var nearCount = 0;
                var farZ = 0f;
                var farU = 0f;
                var farCount = 0;

                for (var k = 0; k < samples.Length; k++)
                {
                    var position = samples[k];

                    if (-position.y <= nearLimit)
                    {
                        nearZ += position.z;
                        nearU += -position.y;
                        nearCount++;
                    }

                    if (-position.y >= farLimit)
                    {
                        farZ += position.z;
                        farU += -position.y;
                        farCount++;
                    }
                }

                if (nearCount == 0 || farCount == 0)
                    return null;

                var directionZ = farZ / farCount - nearZ / nearCount;
                var directionU = farU / farCount - nearU / nearCount;
                var length = Mathf.Sqrt(directionZ * directionZ + directionU * directionU);

                if (length < MinExtent)
                    return null;

                directionZ /= length;
                directionU /= length;

                if (directionU < MinDownwardComponent)
                    return null;

                var parameters = new float[indices.Length];

                for (var k = 0; k < parameters.Length; k++)
                    parameters[k] = -positions[indices[k]].y / directionU;

                return new Extension(
                    positions, indices, parameters, rigidEndFraction, null, null, 0f, 0f, 0f, directionZ, directionU);
            }

            private static bool IsCircular(float[] radii)
            {
                var mean = 0f;

                for (var k = 0; k < radii.Length; k++)
                    mean += radii[k];

                mean /= radii.Length;

                var squaredError = 0f;

                for (var k = 0; k < radii.Length; k++)
                    squaredError += (radii[k] - mean) * (radii[k] - mean);

                return mean > MinExtent && Mathf.Sqrt(squaredError / radii.Length) <= MaxFitErrorRatio * mean;
            }

            private static float FindExitAngle(float radius, float centerU, float nearAngle)
            {
                var squared = radius * radius - centerU * centerU;
                var reach = squared > 0f ? Mathf.Sqrt(squared) : 0f;
                var first = Mathf.Atan2(-centerU, reach);
                var second = Mathf.Atan2(-centerU, -reach);

                return Mathf.Abs(WrapAngle(first - nearAngle)) <= Mathf.Abs(WrapAngle(second - nearAngle))
                    ? first
                    : second;
            }

            private static float WrapAngle(float angle)
            {
                while (angle > Mathf.PI)
                    angle -= 2f * Mathf.PI;

                while (angle <= -Mathf.PI)
                    angle += 2f * Mathf.PI;

                return angle;
            }

            private float GetDelta(float parameter, float extra)
            {
                if (parameter <= 0f || _rigidStart <= 0f)
                    return 0f;

                var total = extra * _maxParameter;

                return parameter >= _rigidStart ? total : total * parameter / _rigidStart;
            }

            private Vector3 Move(int k, float extra)
            {
                var position = _positions[_indices[k]];
                var delta = GetDelta(_parameters[k], extra);

                if (delta == 0f)
                    return position;

                if (!_isArc)
                {
                    return new Vector3(
                        position.x,
                        -(-position.y + _directionU * delta),
                        position.z + _directionZ * delta);
                }

                var angle = _angles[k] + _sign * delta;

                return new Vector3(
                    position.x,
                    -(_centerU + _radii[k] * Mathf.Sin(angle)),
                    _centerZ + _radii[k] * Mathf.Cos(angle));
            }
        }
    }
}
