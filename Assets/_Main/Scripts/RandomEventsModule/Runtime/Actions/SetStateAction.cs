using System.Threading;
using Cysharp.Threading.Tasks;

namespace RandomEventsModule
{
    /// <summary>Writes a number into the saved state store under one key.</summary>
    /// <remarks>
    /// The step is how an event records a fact about itself — that it happened, how many times, that
    /// it is running — for conditions and chances to read later. Adding to a key instead of setting it
    /// makes the same action a counter.
    /// </remarks>
    public class SetStateAction : RandomEventActionBase
    {
        private readonly IRandomEventStateStore _store;
        private readonly string _key;
        private readonly int _value;
        private readonly bool _isAdded;

        /// <summary>Creates the action over the key it writes.</summary>
        public SetStateAction(IRandomEventStateStore store, string key, int value, bool isAdded)
        {
            _store = store;
            _key = key;
            _value = value;
            _isAdded = isAdded;
        }

        /// <inheritdoc/>
        public override UniTask<bool> ExecuteAsync(CancellationToken token)
        {
            if (_isAdded)
                _store.AddInt(_key, _value);
            else
                _store.SetInt(_key, _value);

            return UniTask.FromResult(true);
        }
    }
}
