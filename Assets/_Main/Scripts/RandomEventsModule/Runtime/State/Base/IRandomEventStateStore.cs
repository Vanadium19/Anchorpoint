namespace RandomEventsModule
{
    /// <summary>Generic key/value storage for random event conditions, actions and signal relays.</summary>
    /// <remarks>
    /// The core has no knowledge of what the keys mean; every plugin defines its own key assets.
    /// A null key is a no-op: setters do nothing, getters return the supplied default.
    /// </remarks>
    public interface IRandomEventStateStore
    {
        /// <summary>Reads an int value, or <paramref name="defaultValue"/> if unset.</summary>
        int GetInt(RandomEventKey key, int defaultValue = 0);

        /// <summary>Writes an int value.</summary>
        void SetInt(RandomEventKey key, int value);

        /// <summary>Adds to the stored int value and returns the new total.</summary>
        int AddInt(RandomEventKey key, int amount);

        /// <summary>Reads a float value, or <paramref name="defaultValue"/> if unset.</summary>
        float GetFloat(RandomEventKey key, float defaultValue = 0f);

        /// <summary>Writes a float value.</summary>
        void SetFloat(RandomEventKey key, float value);

        /// <summary>Adds to the stored float value and returns the new total.</summary>
        float AddFloat(RandomEventKey key, float amount);

        /// <summary>Reads a bool value, or <paramref name="defaultValue"/> if unset.</summary>
        bool GetBool(RandomEventKey key, bool defaultValue = false);

        /// <summary>Writes a bool value.</summary>
        void SetBool(RandomEventKey key, bool value);

        /// <summary>Whether any value is stored under the key.</summary>
        bool HasKey(RandomEventKey key);

        /// <summary>Removes any value stored under the key.</summary>
        void Remove(RandomEventKey key);

        /// <summary>Removes every stored value.</summary>
        void Clear();
    }
}
