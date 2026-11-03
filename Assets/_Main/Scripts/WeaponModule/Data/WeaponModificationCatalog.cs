using System;
using System.Collections.Generic;
using UnityEngine;

namespace WeaponModule
{
    /// <summary>
    /// Every weapon modification of the game. Which weapon takes which modification is not listed here: it is
    /// derived from the socket types on the weapon view. The list is kept in sync with the project by the editor.
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponModificationCatalog", menuName = "Configs/Weapon Modification Catalog")]
    public class WeaponModificationCatalog : ScriptableObject
    {
        [SerializeField] private WeaponModificationConfig[] modifications = Array.Empty<WeaponModificationConfig>();

        public IReadOnlyList<WeaponModificationConfig> Modifications =>
            modifications ?? Array.Empty<WeaponModificationConfig>();

        /// <summary>
        /// Finds the modification with the given stable id.
        /// </summary>
        /// <param name="id">The id stored in saves.</param>
        /// <returns>The modification, or null if the id is empty or unknown.</returns>
        public WeaponModificationConfig FindById(string id)
        {
            if (string.IsNullOrEmpty(id) || modifications == null)
                return null;

            for (var i = 0; i < modifications.Length; i++)
            {
                var modification = modifications[i];

                if (modification != null && modification.Id == id)
                    return modification;
            }

            return null;
        }
    }
}
