using System;
using System.Collections.Generic;

namespace RandomEventsModule
{
    /// <summary>In-memory <see cref="IRandomEventStateStore"/>, keyed by the raw string key.</summary>
    public class RandomEventStateStore : IRandomEventStateStore, IRandomEventsState
    {
        private readonly Dictionary<string, int> _ints = new();
        private readonly Dictionary<string, float> _floats = new();
        private readonly Dictionary<string, bool> _flags = new();

        /// <inheritdoc/>
        public int GetInt(string key, int defaultValue = 0) =>
            !string.IsNullOrEmpty(key) && _ints.TryGetValue(key, out var value) ? value : defaultValue;

        /// <inheritdoc/>
        public void SetInt(string key, int value)
        {
            if (string.IsNullOrEmpty(key))
                return;

            _ints[key] = value;
        }

        /// <inheritdoc/>
        public int AddInt(string key, int amount)
        {
            if (string.IsNullOrEmpty(key))
                return 0;

            var newValue = (_ints.TryGetValue(key, out var value) ? value : 0) + amount;
            _ints[key] = newValue;

            return newValue;
        }

        /// <inheritdoc/>
        public float GetFloat(string key, float defaultValue = 0f) =>
            !string.IsNullOrEmpty(key) && _floats.TryGetValue(key, out var value) ? value : defaultValue;

        /// <inheritdoc/>
        public void SetFloat(string key, float value)
        {
            if (string.IsNullOrEmpty(key))
                return;

            _floats[key] = value;
        }

        /// <inheritdoc/>
        public float AddFloat(string key, float amount)
        {
            if (string.IsNullOrEmpty(key))
                return 0f;

            var newValue = (_floats.TryGetValue(key, out var value) ? value : 0f) + amount;
            _floats[key] = newValue;

            return newValue;
        }

        /// <inheritdoc/>
        public bool GetBool(string key, bool defaultValue = false) =>
            !string.IsNullOrEmpty(key) && _flags.TryGetValue(key, out var value) ? value : defaultValue;

        /// <inheritdoc/>
        public void SetBool(string key, bool value)
        {
            if (string.IsNullOrEmpty(key))
                return;

            _flags[key] = value;
        }

        /// <inheritdoc/>
        public bool HasKey(string key) =>
            !string.IsNullOrEmpty(key) && (_ints.ContainsKey(key) || _floats.ContainsKey(key) || _flags.ContainsKey(key));

        /// <inheritdoc/>
        public void Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
                return;

            _ints.Remove(key);
            _floats.Remove(key);
            _flags.Remove(key);
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _ints.Clear();
            _floats.Clear();
            _flags.Clear();
        }

        /// <inheritdoc/>
        public RandomEventsMemento CreateSnapshot() => new()
        {
            Ints = new Dictionary<string, int>(_ints),
            Floats = new Dictionary<string, float>(_floats),
            Flags = new Dictionary<string, bool>(_flags),
            SavedAtUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        };

        /// <inheritdoc/>
        public void RestoreSnapshot(RandomEventsMemento memento)
        {
            _ints.Clear();
            _floats.Clear();
            _flags.Clear();

            if (memento == null)
                return;

            if (memento.Ints != null)
                foreach (var pair in memento.Ints)
                    _ints[pair.Key] = pair.Value;

            if (memento.Floats != null)
                foreach (var pair in memento.Floats)
                    _floats[pair.Key] = pair.Value;

            if (memento.Flags != null)
                foreach (var pair in memento.Flags)
                    _flags[pair.Key] = pair.Value;
        }
    }
}
