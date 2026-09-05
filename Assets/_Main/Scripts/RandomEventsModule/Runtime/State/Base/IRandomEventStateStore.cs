namespace RandomEventsModule
{
    /// <summary>Generic key/value storage for random event conditions, actions and signal relays.</summary>
    /// <remarks>
    /// The core has no knowledge of what the keys mean; every plugin defines its own key strings.
    /// A null or empty key is a no-op: setters do nothing, getters return the supplied default.
    /// </remarks>
    public interface IRandomEventStateStore
    {
        /// <summary>Reads an int value, or <paramref name="defaultValue"/> if unset.</summary>
        int GetInt(string key, int defaultValue = 0);

        /// <summary>Writes an int value.</summary>
        void SetInt(string key, int value);

        /// <summary>Adds to the stored int value and returns the new total.</summary>
        int AddInt(string key, int amount);

        /// <summary>Reads a float value, or <paramref name="defaultValue"/> if unset.</summary>
        float GetFloat(string key, float defaultValue = 0f);

        /// <summary>Writes a float value.</summary>
        void SetFloat(string key, float value);

        /// <summary>Adds to the stored float value and returns the new total.</summary>
        float AddFloat(string key, float amount);

        /// <summary>Reads a bool value, or <paramref name="defaultValue"/> if unset.</summary>
        bool GetBool(string key, bool defaultValue = false);

        /// <summary>Writes a bool value.</summary>
        void SetBool(string key, bool value);

        /// <summary>Whether any value is stored under the key.</summary>
        bool HasKey(string key);

        /// <summary>Removes any value stored under the key.</summary>
        void Remove(string key);

        /// <summary>Removes every stored value.</summary>
        void Clear();
    }
}
