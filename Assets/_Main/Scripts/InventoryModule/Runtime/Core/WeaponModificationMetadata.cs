using System;
using System.Collections.Generic;

namespace InventoryModule
{
    /// <summary>
    /// Stores IDs of the modifications installed on a single weapon item instance.
    /// </summary>
    [Serializable]
    public class WeaponModificationMetadata : InventoryMetadata
    {
        private readonly List<string> _installedModificationIDs = new();

        public IReadOnlyList<string> InstalledModificationIDs => _installedModificationIDs;

        public bool Contains(string modificationID) => _installedModificationIDs.Contains(modificationID);

        public bool TryAdd(string modificationID)
        {
            if (string.IsNullOrEmpty(modificationID) || Contains(modificationID))
                return false;

            _installedModificationIDs.Add(modificationID);
            return true;
        }

        public void Remove(string modificationID) => _installedModificationIDs.Remove(modificationID);
    }
}
