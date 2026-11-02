using System;
using System.Collections.Generic;

namespace WeaponModule
{
    /// <summary>
    /// Pure stat composition: value = (base + sum(Add)) * product(1 + Multiply), floored at 0.
    /// The result does not depend on modifier order. This is the single place to add stacking rules later.
    /// </summary>
    public static class WeaponStatComposer
    {
        private static readonly int StatCount = Enum.GetValues(typeof(WeaponStat)).Length;

        /// <summary>
        /// Applies all modifiers to the base stats and returns the composed stats.
        /// </summary>
        public static WeaponStats Compose(WeaponStats baseStats, IEnumerable<WeaponStatModifier> modifiers)
        {
            if (baseStats == null)
                throw new ArgumentNullException(nameof(baseStats));

            if (modifiers == null)
                throw new ArgumentNullException(nameof(modifiers));

            var ordered = new List<WeaponStatModifier>(modifiers);
            ordered.Sort(CompareModifiers);

            var sums = new double[StatCount];
            var products = new double[StatCount];

            for (var i = 0; i < StatCount; i++)
                products[i] = 1d;

            foreach (var modifier in ordered)
            {
                var index = (int)modifier.Stat;

                if (index < 0 || index >= StatCount)
                    continue;

                if (modifier.Operation == WeaponStatOperation.Add)
                    sums[index] += modifier.Value;
                else
                    products[index] *= 1d + modifier.Value;
            }

            return new WeaponStats(
                Resolve(baseStats, WeaponStat.Damage, sums, products),
                Resolve(baseStats, WeaponStat.MagazineCapacity, sums, products),
                Resolve(baseStats, WeaponStat.HipSpreadAngle, sums, products),
                Resolve(baseStats, WeaponStat.AimSpreadAngle, sums, products));
        }

        private static float Resolve(WeaponStats baseStats, WeaponStat stat, double[] sums, double[] products)
        {
            var index = (int)stat;
            var value = (baseStats.Get(stat) + sums[index]) * products[index];

            return (float)Math.Max(0d, value);
        }

        private static int CompareModifiers(WeaponStatModifier left, WeaponStatModifier right)
        {
            var result = left.Stat.CompareTo(right.Stat);

            if (result != 0)
                return result;

            result = left.Operation.CompareTo(right.Operation);

            return result != 0 ? result : left.Value.CompareTo(right.Value);
        }
    }
}
