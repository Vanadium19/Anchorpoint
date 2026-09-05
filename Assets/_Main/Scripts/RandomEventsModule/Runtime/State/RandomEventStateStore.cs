using System;
using System.Collections.Generic;

namespace RandomEventsModule
{
    /// <summary>In-memory <see cref="IRandomEventStateStore"/>, keyed by <see cref="RandomEventKey.PersistentId"/>.</summary>
    public class RandomEventStateStore : IRandomEventStateStore, IRandomEventsState
    {
        private readonly Dictionary<string, int> _ints = new();
        private readonly Dictionary<string, float> _floats = new();
        private readonly Dictionary<string, bool> _flags = new();

        /// <inheritdoc/>
        public int GetInt(RandomEventKey key, int defaultValue = 0)
        {
            var id = Resolve(key);

            return id != null && _ints.TryGetValue(id, out var value) ? value : defaultValue;
        }

        /// <inheritdoc/>
        public void SetInt(RandomEventKey key, int value)
        {
            var id = Resolve(key);

            if (id == null)
                return;

            _ints[id] = value;
        }

        /// <inheritdoc/>
        public int AddInt(RandomEventKey key, int amount)
        {
            var id = Resolve(key);

            if (id == null)
                return 0;

            var newValue = (_ints.TryGetValue(id, out var value) ? value : 0) + amount;
            _ints[id] = newValue;

            return newValue;
        }

        /// <inheritdoc/>
        public float GetFloat(RandomEventKey key, float defaultValue = 0f)
        {
            var id = Resolve(key);

            return id != null && _floats.TryGetValue(id, out var value) ? value : defaultValue;
        }

        /// <inheritdoc/>
        public void SetFloat(RandomEventKey key, float value)
        {
            var id = Resolve(key);

            if (id == null)
                return;

            _floats[id] = value;
        }

        /// <inheritdoc/>
        public float AddFloat(RandomEventKey key, float amount)
        {
            var id = Resolve(key);

            if (id == null)
                return 0f;

            var newValue = (_floats.TryGetValue(id, out var value) ? value : 0f) + amount;
            _floats[id] = newValue;

            return newValue;
        }

        /// <inheritdoc/>
        public bool GetBool(RandomEventKey key, bool defaultValue = false)
        {
            var id = Resolve(key);

            return id != null && _flags.TryGetValue(id, out var value) ? value : defaultValue;
        }

        /// <inheritdoc/>
        public void SetBool(RandomEventKey key, bool value)
        {
            var id = Resolve(key);

            if (id == null)
                return;

            _flags[id] = value;
        }

        /// <inheritdoc/>
        public bool HasKey(RandomEventKey key)
        {
            var id = Resolve(key);

            return id != null && (_ints.ContainsKey(id) || _floats.ContainsKey(id) || _flags.ContainsKey(id));
        }

        /// <inheritdoc/>
        public void Remove(RandomEventKey key)
        {
            var id = Resolve(key);

            if (id == null)
                return;

            _ints.Remove(id);
            _floats.Remove(id);
            _flags.Remove(id);
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

        private static string Resolve(RandomEventKey key)
        {
            if (key == null)
                return null;

            var persistentId = key.PersistentId;

            return string.IsNullOrEmpty(persistentId) ? null : persistentId;
        }
    }
}
