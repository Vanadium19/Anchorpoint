namespace RandomEventsModule
{
    /// <summary>Runtime form of one <see cref="RandomEventTrigger"/>: its resolved source, conditions, chance and picker.</summary>
    /// <remarks>Created once per scope and kept for the runner's lifetime, so a source or picker can remember what it did last time.</remarks>
    public class RandomEventTriggerInstance
    {
        private readonly IRandomEventTriggerSource _source;
        private readonly RandomEventConditionGroup _conditions;
        private readonly IRandomEventChance _chance;

        private float _pollTimer;

        /// <summary>Creates the runtime trigger from already-resolved parts.</summary>
        public RandomEventTriggerInstance(
            IRandomEventTriggerSource source,
            RandomEventConditionGroup conditions,
            IRandomEventChance chance,
            RandomEventPool pool,
            IRandomEventPicker picker)
        {
            _source = source;
            _conditions = conditions;
            _chance = chance;
            Pool = pool;
            Picker = picker;
        }

        /// <summary>The pool this trigger picks from.</summary>
        public RandomEventPool Pool { get; }

        /// <summary>The algorithm this trigger picks with.</summary>
        public IRandomEventPicker Picker { get; }

        /// <summary>Advances the periodic timer and returns whether a periodic poll is due.</summary>
        public bool IsPollDue(float deltaTime)
        {
            if (_source.PollIntervalSeconds <= 0f)
                return false;

            _pollTimer += deltaTime;

            if (_pollTimer < _source.PollIntervalSeconds)
                return false;

            _pollTimer = 0f;

            return true;
        }

        /// <summary>Whether a reported signal should poll this trigger immediately.</summary>
        public bool HandlesSignal(string signal) => _source.HandlesSignal(signal);

        /// <summary>Consumes the source's readiness, checks conditions and rolls the chance.</summary>
        public bool TryFire() => _source.TryFire() && _conditions.IsMet() && (_chance == null || _chance.Roll());
    }
}
