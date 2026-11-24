using System;
using UnityEngine;

namespace EnemyModule
{
    /// <summary>
    /// The drone's charge: lights a fuse near the target and explodes either on schedule or when the drone is shot down.
    /// </summary>
    public interface IDroneExplosive
    {
        /// <summary>
        /// Raised once when the fuse is lit.
        /// </summary>
        event Action FuseLit;

        /// <summary>
        /// Raised once with the position and radius of the explosion, however it was caused.
        /// </summary>
        event Action<Vector3, float> Detonated;

        bool IsFuseLit { get; }

        /// <summary>
        /// Lights the fuse. Does nothing when it is already lit.
        /// </summary>
        void LightFuse();

        /// <summary>
        /// Explodes at full radius, as when the drone reaches the player.
        /// </summary>
        void Detonate();
    }
}
