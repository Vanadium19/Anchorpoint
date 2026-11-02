using System;
using System.Collections.Generic;

namespace WeaponModule
{
    /// <summary>
    /// A weapon config with a set of installed modifications and the resulting composed stats.
    /// </summary>
    public sealed class WeaponBuild
    {
        private readonly List<WeaponModificationConfig> _modifications;

        private WeaponBuild(List<WeaponModificationConfig> modifications, WeaponStats stats)
        {
            _modifications = modifications;
            Stats = stats;
        }

        public IReadOnlyList<WeaponModificationConfig> Modifications => _modifications;

        public WeaponStats Stats { get; }

        /// <summary>
        /// Composes the stats of the config with the modifiers of every given modification. Null entries are skipped.
        /// </summary>
        public static WeaponBuild Create(WeaponConfig config, IEnumerable<WeaponModificationConfig> modifications)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (modifications == null)
                throw new ArgumentNullException(nameof(modifications));

            var installed = new List<WeaponModificationConfig>();
            var modifiers = new List<WeaponStatModifier>();

            foreach (var modification in modifications)
            {
                if (modification == null)
                    continue;

                installed.Add(modification);

                foreach (var modifier in modification.Modifiers)
                    modifiers.Add(modifier);
            }

            return new WeaponBuild(installed, WeaponStatComposer.Compose(config.BaseStats, modifiers));
        }

        /// <summary>
        /// Returns a new build with one more modification, for previewing an install. This build is unchanged.
        /// </summary>
        public WeaponBuild With(WeaponConfig config, WeaponModificationConfig modification)
        {
            var installed = new List<WeaponModificationConfig>(_modifications);

            if (modification != null)
                installed.Add(modification);

            return Create(config, installed);
        }
    }
}
