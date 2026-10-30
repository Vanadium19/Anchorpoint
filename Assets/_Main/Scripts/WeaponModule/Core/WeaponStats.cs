using System.Collections.Generic;
using UnityEngine;

namespace WeaponModule
{
    /// <summary>
    /// Effective weapon stats: the values of <see cref="WeaponConfig"/> with installed modifications applied.
    /// </summary>
    public class WeaponStats
    {
        public WeaponStats(WeaponConfig config, IReadOnlyList<WeaponModificationConfig> modifications)
        {
            var damage = config.Damage;
            var magazineCapacity = config.MagazineCapacity;
            var aimInaccuracy = config.AimInaccuracy;
            var aimZoom = 1f;

            foreach (var modification in modifications)
            {
                damage *= modification.DamageMultiplier;
                magazineCapacity += modification.MagazineCapacityBonus;
                aimInaccuracy *= modification.AimInaccuracyMultiplier;
                aimZoom *= modification.AimZoom;
            }

            Damage = damage;
            MagazineCapacity = Mathf.Max(1, magazineCapacity);
            HipInaccuracy = config.HipInaccuracy;
            AimInaccuracy = aimInaccuracy;
            AimZoom = aimZoom;
        }

        public float Damage { get; }
        public int MagazineCapacity { get; }
        public float HipInaccuracy { get; }
        public float AimInaccuracy { get; }
        public float AimZoom { get; }

        public float GetInaccuracy(bool isAiming) => isAiming ? AimInaccuracy : HipInaccuracy;
    }
}
