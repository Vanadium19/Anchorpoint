using System;

namespace WeaponModule
{
    /// <summary>
    /// Immutable set of weapon stat values. Has no Unity runtime dependency.
    /// </summary>
    public sealed class WeaponStats
    {
        private readonly float _damage;
        private readonly float _magazineCapacity;
        private readonly float _hipSpreadAngle;
        private readonly float _aimSpreadAngle;

        public WeaponStats(float damage, float magazineCapacity, float hipSpreadAngle, float aimSpreadAngle)
        {
            _damage = damage;
            _magazineCapacity = magazineCapacity;
            _hipSpreadAngle = hipSpreadAngle;
            _aimSpreadAngle = aimSpreadAngle;
        }

        public float Damage => _damage;
        public int MagazineCapacity => Math.Max(1, (int)Math.Round(_magazineCapacity, MidpointRounding.AwayFromZero));
        public float HipSpreadAngle => _hipSpreadAngle;
        public float AimSpreadAngle => _aimSpreadAngle;

        /// <summary>
        /// Returns the raw value of a stat. Magazine capacity is not rounded here.
        /// </summary>
        public float Get(WeaponStat stat) => stat switch
        {
            WeaponStat.Damage => _damage,
            WeaponStat.MagazineCapacity => _magazineCapacity,
            WeaponStat.HipSpreadAngle => _hipSpreadAngle,
            WeaponStat.AimSpreadAngle => _aimSpreadAngle,
            _ => throw new ArgumentOutOfRangeException(nameof(stat), stat, null)
        };
    }
}
