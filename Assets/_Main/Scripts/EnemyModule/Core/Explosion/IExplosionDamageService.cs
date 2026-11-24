using UnityEngine;

namespace EnemyModule
{
    /// <summary>
    /// Applies explosion damage to the player.
    /// </summary>
    public interface IExplosionDamageService
    {
        /// <summary>
        /// Damages the player when the player is within <paramref name="radius"/> of <paramref name="center"/>.
        /// </summary>
        /// <remarks>
        /// Damage falls off linearly from <paramref name="maxDamage"/> at the center to zero at the edge of the radius.
        /// Other enemies are never affected.
        /// </remarks>
        void ApplyToPlayer(Vector3 center, float radius, float maxDamage);
    }
}
