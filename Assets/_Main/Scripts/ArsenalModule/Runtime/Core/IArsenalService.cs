using System;
using System.Collections.Generic;
using InventoryModule;
using WeaponModule;

namespace ArsenalModule
{
    /// <summary>
    /// Finds the player's weapons and installs modifications on them at the Arsenal.
    /// </summary>
    public interface IArsenalService
    {
        /// <summary>
        /// Raised after a modification has been installed and the weapon build has changed.
        /// </summary>
        event Action BuildChanged;

        /// <summary>
        /// Returns every weapon the player carries: equipment slots, the main grid and, recursively,
        /// the grids of the containers found there.
        /// </summary>
        /// <returns>The weapon items, without duplicates.</returns>
        IReadOnlyList<ItemTable> GetWeapons();

        /// <summary>
        /// Returns the number of items of the given kind the player holds.
        /// </summary>
        /// <param name="item">The item kind to count.</param>
        /// <returns>The total count across the whole inventory.</returns>
        int GetOwnedCount(ItemDataSo item);

        /// <summary>
        /// Returns the catalog modifications that fit the weapon: the ones whose socket type the weapon view has.
        /// </summary>
        /// <param name="weapon">The weapon item.</param>
        /// <returns>The modifications, or an empty list if the item is not a weapon.</returns>
        IReadOnlyList<WeaponModificationConfig> GetAvailableModifications(ItemTable weapon);

        /// <summary>
        /// Returns the weapon build composed from the modifications installed on the weapon.
        /// </summary>
        /// <param name="weapon">The weapon item.</param>
        /// <returns>The build, or null if the item is not a weapon or has no weapon config.</returns>
        WeaponBuild GetBuild(ItemTable weapon);

        /// <summary>
        /// Checks whether the modification can be installed on the weapon.
        /// </summary>
        /// <param name="weapon">The weapon item.</param>
        /// <param name="modification">The modification to check.</param>
        /// <returns>The install status.</returns>
        ArsenalInstallStatus GetInstallStatus(ItemTable weapon, WeaponModificationConfig modification);

        /// <summary>
        /// Installs the modification: removes every cost item, records the modification on the weapon
        /// and re-applies the weapon build.
        /// </summary>
        /// <remarks>
        /// The operation is atomic. If <see cref="GetInstallStatus"/> is not
        /// <see cref="ArsenalInstallStatus.Available"/>, nothing is removed or changed.
        /// </remarks>
        /// <param name="weapon">The weapon item.</param>
        /// <param name="modification">The modification to install.</param>
        /// <returns>True if the modification was installed.</returns>
        bool TryInstall(ItemTable weapon, WeaponModificationConfig modification);
    }
}
