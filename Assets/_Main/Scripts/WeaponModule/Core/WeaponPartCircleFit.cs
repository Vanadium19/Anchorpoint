using System;
using System.Collections.Generic;
using UnityEngine;

namespace WeaponModule
{
    /// <summary>
    /// Fits a circle to the vertices of a curved part in the plane of its curve (Z forward, U down the part).
    /// The part is a band around the circle, so the start comes from the middle line of the band and is then
    /// refined on all vertices, treating the two walls of the band as two radii of one circle.
    /// </summary>
    internal sealed class WeaponPartCircleFit
    {
        private const float MinExtent = 1e-6f;
        private const float MaxStepRatio = 0.5f;
        private const float EndShare = 0.15f;
        private const double BisectorFirstRatio = 0.25;
        private const double BisectorGrowth = 1.8;
        private const int BisectorStarts = 8;
        private const double MinDeterminantRatio = 1e-9;
        private const int Bins = 16;
        private const int MinCenterLinePoints = 4;
        private const int RefineIterations = 12;
        private const int WallCount = 2;
        private const int SplitIterations = 4;

        private readonly Vector3[] _positions;
        private readonly int[] _indices;
        private readonly double[] _radii;
        private readonly double[] _weights;
        private readonly double[] _directionsZ;
        private readonly double[] _directionsU;
        private readonly int[] _walls;
        private readonly double[] _wallMeans = new double[WallCount];

        private double _meanRadius;

        private WeaponPartCircleFit(Vector3[] positions, int[] indices)
        {
            _positions = positions;
            _indices = indices;
            _radii = new double[indices.Length];
            _weights = CreateWeights(positions, indices);
            _directionsZ = new double[indices.Length];
            _directionsU = new double[indices.Length];
            _walls = new int[indices.Length];
        }

        /// <summary>
        /// Finds the center of the circle that the points lie around.
        /// </summary>
        /// <param name="points">Positions in the space of the mount, depth is -Y.</param>
        /// <param name="centerZ">Z of the center.</param>
        /// <param name="centerU">Depth of the center, -Y.</param>
        /// <returns>False if the points do not allow a fit, for example for a straight part.</returns>
        public static bool TryFit(Vector3[] points, out float centerZ, out float centerU)
        {
            var indices = new int[points.Length];

            for (var i = 0; i < indices.Length; i++)
                indices[i] = i;

            return TryFit(points, indices, out centerZ, out centerU);
        }

        /// <summary>
        /// Finds the center of the circle that the listed vertices lie around.
        /// </summary>
        /// <param name="positions">Vertex positions in the space of the mount, depth is -Y.</param>
        /// <param name="indices">The vertices of the part.</param>
        /// <param name="centerZ">Z of the center.</param>
        /// <param name="centerU">Depth of the center, -Y.</param>
        /// <returns>False if the vertices do not allow a fit, for example for a straight part.</returns>
        public static bool TryFit(Vector3[] positions, int[] indices, out float centerZ, out float centerU)
        {
            centerZ = 0f;
            centerU = 0f;

            var fit = new WeaponPartCircleFit(positions, indices);
            var bestError = double.MaxValue;

            if (TryFindCenterLine(positions, indices, out var lineZ, out var lineU)
                && TryFitCenterLine(lineZ, lineU, out var startZ, out var startU))
            {
                bestError = fit.Refine(startZ, startU, out var refinedZ, out var refinedU);
                centerZ = (float)refinedZ;
                centerU = (float)refinedU;
            }

            foreach (var start in fit.FindBisectorStarts())
            {
                var error = fit.Refine(start.Item1, start.Item2, out var refinedZ, out var refinedU);

                if (error < bestError)
                {
                    bestError = error;
                    centerZ = (float)refinedZ;
                    centerU = (float)refinedU;
                }
            }

            return bestError < double.MaxValue
                && !float.IsNaN(centerZ) && !float.IsNaN(centerU)
                && !float.IsInfinity(centerZ) && !float.IsInfinity(centerU);
        }

        private static bool TryFindCenterLine(
            Vector3[] positions,
            int[] indices,
            out List<double> lineZ,
            out List<double> lineU)
        {
            lineZ = new List<double>();
            lineU = new List<double>();

            var minZ = float.MaxValue;
            var maxZ = float.MinValue;
            var minU = float.MaxValue;
            var maxU = float.MinValue;

            for (var k = 0; k < indices.Length; k++)
            {
                var position = positions[indices[k]];

                minZ = Mathf.Min(minZ, position.z);
                maxZ = Mathf.Max(maxZ, position.z);
                minU = Mathf.Min(minU, -position.y);
                maxU = Mathf.Max(maxU, -position.y);
            }

            var alongU = maxU - minU >= maxZ - minZ;
            var alongMin = alongU ? minU : minZ;
            var alongRange = alongU ? maxU - minU : maxZ - minZ;

            if (alongRange < MinExtent)
                return false;

            var sums = new double[Bins];
            var counts = new int[Bins];
            var lows = new float[Bins];
            var highs = new float[Bins];

            for (var bin = 0; bin < Bins; bin++)
            {
                lows[bin] = float.MaxValue;
                highs[bin] = float.MinValue;
            }

            for (var k = 0; k < indices.Length; k++)
            {
                var position = positions[indices[k]];
                var along = alongU ? -position.y : position.z;
                var across = alongU ? position.z : -position.y;
                var bin = Mathf.Clamp((int)((along - alongMin) / alongRange * Bins), 0, Bins - 1);

                sums[bin] += along;
                counts[bin]++;
                lows[bin] = Mathf.Min(lows[bin], across);
                highs[bin] = Mathf.Max(highs[bin], across);
            }

            for (var bin = 0; bin < Bins; bin++)
            {
                if (counts[bin] < 2)
                    continue;

                var along = sums[bin] / counts[bin];
                var across = (lows[bin] + highs[bin]) * 0.5;

                lineZ.Add(alongU ? across : along);
                lineU.Add(alongU ? along : across);
            }

            return lineZ.Count >= MinCenterLinePoints;
        }

        private static bool TryFitCenterLine(List<double> lineZ, List<double> lineU, out double centerZ, out double centerU)
        {
            centerZ = 0.0;
            centerU = 0.0;

            var count = lineZ.Count;
            var meanZ = 0.0;
            var meanU = 0.0;

            for (var k = 0; k < count; k++)
            {
                meanZ += lineZ[k];
                meanU += lineU[k];
            }

            meanZ /= count;
            meanU /= count;

            double sumZz = 0.0, sumUu = 0.0, sumUz = 0.0, sumBz = 0.0, sumBu = 0.0;

            for (var k = 0; k < count; k++)
            {
                var z = lineZ[k] - meanZ;
                var u = lineU[k] - meanU;
                var squared = z * z + u * u;

                sumZz += z * z;
                sumUu += u * u;
                sumUz += u * z;
                sumBz += z * squared;
                sumBu += u * squared;
            }

            var determinant = sumZz * sumUu - sumUz * sumUz;

            if (sumZz <= 0.0 || sumUu <= 0.0 || determinant <= MinDeterminantRatio * sumZz * sumUu)
                return false;

            centerZ = meanZ + 0.5 * (sumBz * sumUu - sumBu * sumUz) / determinant;
            centerU = meanU + 0.5 * (sumBu * sumZz - sumBz * sumUz) / determinant;

            return !double.IsNaN(centerZ) && !double.IsNaN(centerU)
                && !double.IsInfinity(centerZ) && !double.IsInfinity(centerU);
        }

        private IEnumerable<(double, double)> FindBisectorStarts()
        {
            var minU = float.MaxValue;
            var maxU = float.MinValue;

            for (var k = 0; k < _indices.Length; k++)
            {
                minU = Mathf.Min(minU, -_positions[_indices[k]].y);
                maxU = Mathf.Max(maxU, -_positions[_indices[k]].y);
            }

            var range = maxU - minU;

            if (range < MinExtent)
                yield break;

            double nearZ = 0.0, nearU = 0.0, farZ = 0.0, farU = 0.0;
            var nearCount = 0;
            var farCount = 0;

            for (var k = 0; k < _indices.Length; k++)
            {
                var position = _positions[_indices[k]];

                if (-position.y <= minU + range * EndShare)
                {
                    nearZ += position.z;
                    nearU += -position.y;
                    nearCount++;
                }

                if (-position.y >= maxU - range * EndShare)
                {
                    farZ += position.z;
                    farU += -position.y;
                    farCount++;
                }
            }

            nearZ /= nearCount;
            nearU /= nearCount;
            farZ /= farCount;
            farU /= farCount;

            var chordZ = farZ - nearZ;
            var chordU = farU - nearU;
            var chordLength = Math.Sqrt(chordZ * chordZ + chordU * chordU);

            if (chordLength < MinExtent)
                yield break;

            var normalZ = -chordU / chordLength;
            var normalU = chordZ / chordLength;
            var middleZ = (nearZ + farZ) * 0.5;
            var middleU = (nearU + farU) * 0.5;

            for (var step = 0; step < BisectorStarts; step++)
            {
                var distance = chordLength * BisectorFirstRatio * Math.Pow(BisectorGrowth, step);

                yield return (middleZ + normalZ * distance, middleU + normalU * distance);
                yield return (middleZ - normalZ * distance, middleU - normalU * distance);
            }
        }

        private double Refine(double startZ, double startU, out double centerZ, out double centerU)
        {
            centerZ = startZ;
            centerU = startU;

            var bestError = Measure(centerZ, centerU);

            for (var iteration = 0; iteration < RefineIterations; iteration++)
            {
                if (!TrySolveStep(out var stepZ, out var stepU))
                    break;

                var stepLength = Math.Sqrt(stepZ * stepZ + stepU * stepU);
                var limit = _meanRadius * MaxStepRatio;

                if (stepLength > limit)
                {
                    stepZ *= limit / stepLength;
                    stepU *= limit / stepLength;
                }

                var candidateError = Measure(centerZ + stepZ, centerU + stepU);

                if (!(candidateError < bestError))
                    break;

                bestError = candidateError;
                centerZ += stepZ;
                centerU += stepU;
            }

            return bestError;
        }

        private double Measure(double centerZ, double centerU)
        {
            var sum = 0.0;
            var weightSum = 0.0;

            for (var k = 0; k < _indices.Length; k++)
            {
                var offsetZ = _positions[_indices[k]].z - centerZ;
                var offsetU = -_positions[_indices[k]].y - centerU;
                var radius = Math.Sqrt(offsetZ * offsetZ + offsetU * offsetU);

                _radii[k] = radius;
                _directionsZ[k] = radius > 0.0 ? -offsetZ / radius : 0.0;
                _directionsU[k] = radius > 0.0 ? -offsetU / radius : 0.0;
                sum += radius * _weights[k];
                weightSum += _weights[k];
            }

            _meanRadius = sum / weightSum;
            SplitIntoWalls();

            var error = 0.0;

            for (var k = 0; k < _indices.Length; k++)
            {
                var difference = _radii[k] - _wallMeans[_walls[k]];

                error += _weights[k] * difference * difference;
            }

            return error;
        }

        private bool TrySolveStep(out double stepZ, out double stepU)
        {
            stepZ = 0.0;
            stepU = 0.0;

            double sumZz = 0.0, sumUu = 0.0, sumUz = 0.0, sumZe = 0.0, sumUe = 0.0;

            for (var wall = 0; wall < WallCount; wall++)
            {
                var weightSum = 0.0;
                var meanZ = 0.0;
                var meanU = 0.0;

                for (var k = 0; k < _indices.Length; k++)
                {
                    if (_walls[k] != wall)
                        continue;

                    weightSum += _weights[k];
                    meanZ += _directionsZ[k] * _weights[k];
                    meanU += _directionsU[k] * _weights[k];
                }

                if (weightSum <= 0.0)
                    continue;

                meanZ /= weightSum;
                meanU /= weightSum;

                for (var k = 0; k < _indices.Length; k++)
                {
                    if (_walls[k] != wall)
                        continue;

                    var z = _directionsZ[k] - meanZ;
                    var u = _directionsU[k] - meanU;
                    var error = _radii[k] - _wallMeans[wall];

                    var weight = _weights[k];

                    sumZz += weight * z * z;
                    sumUu += weight * u * u;
                    sumUz += weight * z * u;
                    sumZe += weight * z * error;
                    sumUe += weight * u * error;
                }
            }

            var determinant = sumZz * sumUu - sumUz * sumUz;

            if (_meanRadius < MinExtent || determinant <= MinDeterminantRatio * sumZz * sumUu)
                return false;

            stepZ = -(sumUu * sumZe - sumUz * sumUe) / determinant;
            stepU = -(sumZz * sumUe - sumUz * sumZe) / determinant;

            return !double.IsNaN(stepZ) && !double.IsNaN(stepU);
        }

        private void SplitIntoWalls()
        {
            var threshold = _meanRadius;

            for (var iteration = 0; iteration < SplitIterations; iteration++)
            {
                var lowSum = 0.0;
                var highSum = 0.0;
                var lowWeight = 0.0;
                var highWeight = 0.0;

                for (var k = 0; k < _radii.Length; k++)
                {
                    if (_radii[k] < threshold)
                    {
                        lowSum += _radii[k] * _weights[k];
                        lowWeight += _weights[k];
                    }
                    else
                    {
                        highSum += _radii[k] * _weights[k];
                        highWeight += _weights[k];
                    }
                }

                if (lowWeight <= 0.0 || highWeight <= 0.0)
                    break;

                threshold = (lowSum / lowWeight + highSum / highWeight) * 0.5;
            }

            var sums = new double[WallCount];
            var weights = new double[WallCount];

            for (var k = 0; k < _radii.Length; k++)
            {
                _walls[k] = _radii[k] < threshold ? 0 : 1;
                sums[_walls[k]] += _radii[k] * _weights[k];
                weights[_walls[k]] += _weights[k];
            }

            for (var wall = 0; wall < WallCount; wall++)
                _wallMeans[wall] = weights[wall] > 0.0 ? sums[wall] / weights[wall] : _meanRadius;
        }

        private static double[] CreateWeights(Vector3[] positions, int[] indices)
        {
            var weights = new double[indices.Length];
            var minU = float.MaxValue;
            var maxU = float.MinValue;

            for (var k = 0; k < indices.Length; k++)
            {
                minU = Mathf.Min(minU, -positions[indices[k]].y);
                maxU = Mathf.Max(maxU, -positions[indices[k]].y);
            }

            var range = maxU - minU;
            var counts = new int[Bins];
            var bins = new int[indices.Length];

            for (var k = 0; k < indices.Length; k++)
            {
                bins[k] = range < MinExtent ? 0 : GetBin(-positions[indices[k]].y, minU, range);
                counts[bins[k]]++;
            }

            for (var k = 0; k < indices.Length; k++)
                weights[k] = 1.0 / counts[bins[k]];

            return weights;
        }

        private static int GetBin(float along, float alongMin, float alongRange) =>
            Mathf.Clamp((int)((along - alongMin) / alongRange * Bins), 0, Bins - 1);
    }
}
