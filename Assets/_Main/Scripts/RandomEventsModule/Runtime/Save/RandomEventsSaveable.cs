using Newtonsoft.Json;
using SaveModule;

namespace RandomEventsModule
{
    /// <summary>Adapts <see cref="IRandomEventsState"/> to the save module's <see cref="ISaveable"/> contract.</summary>
    public class RandomEventsSaveable : ISaveable
    {
        private const string Key = "random_events";

        private readonly IRandomEventsState _state;

        /// <summary>Creates the saveable over the given state.</summary>
        public RandomEventsSaveable(IRandomEventsState state)
        {
            _state = state;
        }

        /// <inheritdoc/>
        public string SaveKey => Key;

        /// <inheritdoc/>
        public string CreateMemento() => JsonConvert.SerializeObject(_state.CreateSnapshot());

        /// <inheritdoc/>
        public void RestoreMemento(string data)
        {
            var memento = JsonConvert.DeserializeObject<RandomEventsMemento>(data);

            if (memento == null)
                return;

            _state.RestoreSnapshot(memento);
        }
    }
}
