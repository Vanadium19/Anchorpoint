using System;
using UnityEngine;

namespace WeaponModule
{
    /// <summary>
    /// Scales the distance of the vertices of a part from the mount Z axis, which makes a barrel or a similar
    /// round part thicker or thinner. Vertices nearer to the axis than the minimum radius stay where they are, so a
    /// bore stays a bore. A vertex never grows past the clearance above or below the axis; a vertex that is
    /// already beyond the clearance is left alone.
    /// </summary>
    [CreateAssetMenu(fileName = "RadialThickenOperation", menuName = "Configs/Weapon Part Operations/Radial Thicken")]
    public class RadialThickenOperation : WeaponPartOperation
    {
        private const float MinAxisDistance = 1e-6f;
        private const float MinAxialExtent = 1e-6f;

        [Tooltip("Factor for the distance of a vertex from the mount Z axis. Above 1 makes the part thicker.")]
        [Min(0.01f)]
        [SerializeField] private float radialScale = 1.2f;
        [Tooltip("Metres. Vertices nearer to the axis than this are not moved.")]
        [Min(0f)]
        [SerializeField] private float minRadius;
        [Tooltip("Share of the part length at each end where the effect fades in. 0 applies it evenly.")]
        [Range(0f, 0.5f)]
        [SerializeField] private float axialFalloff;

        public float RadialScale => radialScale;
        public float MinRadius => minRadius;
        public float AxialFalloff => axialFalloff;

        /// <summary>
        /// Moves the positions away from or towards the Z axis. The positions are in the space of the mount and
        /// are changed in place.
        /// </summary>
        /// <param name="positions">Vertex positions in the space of the mount.</param>
        /// <param name="radialScale">Factor for the distance from the Z axis.</param>
        /// <param name="minRadius">Vertices nearer to the axis than this do not move.</param>
        /// <param name="axialFalloff">Share of the length at each end where the effect fades in, 0 to 0.5.</param>
        /// <param name="clearanceAbove">Room above the axis in metres, 0 for unlimited.</param>
        /// <param name="clearanceBelow">Room below the axis in metres, 0 for unlimited.</param>
        public static void Thicken(
            Vector3[] positions,
            float radialScale,
            float minRadius,
            float axialFalloff,
            float clearanceAbove,
            float clearanceBelow)
        {
            if (positions == null)
                throw new ArgumentNullException(nameof(positions));

            if (!(radialScale > 0f) || float.IsInfinity(radialScale))
                throw new ArgumentOutOfRangeException(nameof(radialScale), radialScale, "The scale must be positive.");

            var falloff = Mathf.Clamp(axialFalloff, 0f, 0.5f);
            var minZ = float.MaxValue;
            var maxZ = float.MinValue;

            if (falloff > 0f)
            {
                for (var i = 0; i < positions.Length; i++)
                {
                    minZ = Mathf.Min(minZ, positions[i].z);
                    maxZ = Mathf.Max(maxZ, positions[i].z);
                }
            }

            for (var i = 0; i < positions.Length; i++)
            {
                var position = positions[i];
                var distance = Mathf.Sqrt(position.x * position.x + position.y * position.y);

                if (distance < MinAxisDistance || distance <= minRadius)
                    continue;

                var weight = GetAxialWeight(position.z, minZ, maxZ, falloff);
                var factor = Mathf.Lerp(1f, radialScale, weight);

                if (factor > 1f)
                    factor = LimitToClearance(position.y, factor, clearanceAbove, clearanceBelow);

                positions[i] = new Vector3(position.x * factor, position.y * factor, position.z);
            }
        }

        /// <inheritdoc/>
        public override Mesh Apply(in WeaponPartContext context)
        {
            var positions = WeaponPartMeshFactory.ReadPositions(in context);

            Thicken(positions, radialScale, minRadius, axialFalloff, context.ClearanceAbove, context.ClearanceBelow);

            return WeaponPartMeshFactory.Create(in context, positions, $"{context.Source.name}_{name}");
        }

        private static float GetAxialWeight(float z, float minZ, float maxZ, float falloff)
        {
            var extent = maxZ - minZ;

            if (falloff <= 0f || extent < MinAxialExtent)
                return 1f;

            var along = Mathf.Clamp01((z - minZ) / extent);
            var fadeIn = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(along / falloff));
            var fadeOut = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((1f - along) / falloff));

            return Mathf.Min(fadeIn, fadeOut);
        }

        private static float LimitToClearance(float y, float factor, float clearanceAbove, float clearanceBelow)
        {
            if (y > 0f && clearanceAbove > 0f)
                return Mathf.Min(factor, Mathf.Max(1f, clearanceAbove / y));

            if (y < 0f && clearanceBelow > 0f)
                return Mathf.Min(factor, Mathf.Max(1f, clearanceBelow / -y));

            return factor;
        }
    }
}
