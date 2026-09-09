using UnityEngine;

namespace RandomEventsModule
{
    /// <summary>A percentage that grows with a counter stored under a string key.</summary>
    /// <remarks>
    /// Rolls at <c>basePercent + stepPercent * counter</c>, capped at <c>maxPercent</c>. Whatever
    /// raises the counter defines what "another chance missed" means — a relay reporting a return
    /// to base, a step of some other event — and an action writing to the same key resets it.
    /// </remarks>
    public class StateScaledChance : IRandomEventChance
    {
        private readonly IRandomEventStateStore _store;
        private readonly string _counterKey;
        private readonly float _basePercent;
        private readonly float _stepPercent;
        private readonly float _maxPercent;

        /// <summary>Creates the chance over one counter key.</summary>
        public StateScaledChance(
            IRandomEventStateStore store,
            string counterKey,
            float basePercent,
            float stepPercent,
            float maxPercent)
        {
            _store = store;
            _counterKey = counterKey;
            _basePercent = basePercent;
            _stepPercent = stepPercent;
            _maxPercent = maxPercent;
        }

        /// <inheritdoc/>
        public bool Roll()
        {
            var percent = Mathf.Clamp(_basePercent + _stepPercent * _store.GetInt(_counterKey), 0f, _maxPercent);

            return Random.value * 100f < percent;
        }
    }
}
