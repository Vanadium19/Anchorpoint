using UnityEngine;

namespace RandomEventsModule
{
    /// <summary>A constant percentage roll.</summary>
    public class FixedChance : IRandomEventChance
    {
        private readonly float _percent;

        /// <summary>Creates the chance with its percentage.</summary>
        public FixedChance(float percent)
        {
            _percent = percent;
        }

        /// <inheritdoc/>
        public bool Roll() => Random.value * 100f < _percent;
    }
}
