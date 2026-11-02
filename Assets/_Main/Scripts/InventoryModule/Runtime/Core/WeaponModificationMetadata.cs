using System;
using System.Collections.Generic;

namespace InventoryModule
{
    /// <summary>
    /// Ids of the weapon modifications installed on a weapon item. Ids are plain strings,
    /// so the inventory does not depend on the weapon module.
    /// </summary>
    [Serializable]
    public class WeaponModificationMetadata : InventoryMetadata
    {
        private readonly List<string> _modificationIds = new();

        public IReadOnlyList<string> ModificationIds => _modificationIds;

        /// <summary>
        /// Adds a modification id. Returns false for an empty or already installed id.
        /// </summary>
        public bool AddModification(string id)
        {
            if (string.IsNullOrEmpty(id) || _modificationIds.Contains(id))
                return false;

            _modificationIds.Add(id);
            return true;
        }
    }
}
