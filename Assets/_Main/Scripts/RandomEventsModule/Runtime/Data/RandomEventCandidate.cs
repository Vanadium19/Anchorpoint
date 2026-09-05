namespace RandomEventsModule
{
    /// <summary>One eligible event handed to a picker, together with the weight its pool gave it.</summary>
    public readonly struct RandomEventCandidate
    {
        /// <summary>Creates the candidate.</summary>
        public RandomEventCandidate(RandomEventDefinition definition, float weight)
        {
            Definition = definition;
            Weight = weight;
        }

        /// <summary>The event that may start.</summary>
        public RandomEventDefinition Definition { get; }

        /// <summary>The weight this pool gave the event.</summary>
        public float Weight { get; }
    }
}
