using System;
using System.Collections.Generic;
using InventoryModule;
using WeaponModule;

namespace ArsenalModule
{
    /// <summary>
    /// Finds the weapons the player owns and installs weapon modifications for a price.
    /// </summary>
    public interface IArsenalService
    {
        /// <summary>
        /// Raised after the state shown on the Arsenal screen has changed, for example when a
        /// modification was installed.
        /// </summary>
        event Action StateChanged;

        /// <summary>
        /// Collects every weapon item the player owns, equipped weapons first.
        /// </summary>
        /// <returns>Unique weapon item instances.</returns>
        IReadOnlyList<ItemTable> GetOwnedWeapons();

        /// <summary>
        /// Lists the modifications the Arsenal sells that the weapon accepts.
        /// </summary>
        /// <param name="config">The Arsenal price configuration.</param>
        /// <param name="weapon">The weapon item instance.</param>
        /// <returns>Prices in the weapon's modification order; empty for a non-weapon or null.</returns>
        IReadOnlyList<ArsenalModificationPrice> GetOffers(ArsenalConfig config, ItemTable weapon);

        /// <summary>
        /// Counts the units of an item the player holds.
        /// </summary>
        /// <param name="item">The item to count.</param>
        /// <returns>The owned amount.</returns>
        int GetOwnedCount(ItemDataSo item);

        /// <summary>
        /// Checks whether the player holds every item the price requires.
        /// </summary>
        /// <param name="price">The price to check.</param>
        /// <returns>True if the price can be paid.</returns>
        bool CanAfford(ArsenalModificationPrice price);

        /// <summary>
        /// Checks whether a modification is installed on the weapon.
        /// </summary>
        /// <param name="weapon">The weapon item instance.</param>
        /// <param name="modification">The modification to check.</param>
        /// <returns>True if the modification is installed.</returns>
        bool IsInstalled(ItemTable weapon, WeaponModificationConfig modification);

        /// <summary>
        /// Checks whether a different modification already occupies the slot of
        /// <paramref name="modification"/>.
        /// </summary>
        /// <param name="weapon">The weapon item instance.</param>
        /// <param name="modification">The modification whose slot is checked.</param>
        /// <returns>True if the slot is taken by another modification.</returns>
        bool IsSlotOccupied(ItemTable weapon, WeaponModificationConfig modification);

        /// <summary>
        /// Checks whether the modification can be installed and paid for right now.
        /// </summary>
        /// <param name="weapon">The weapon item instance.</param>
        /// <param name="price">The price of the modification.</param>
        /// <returns>True if the player can afford it and the weapon accepts it.</returns>
        bool CanInstall(ItemTable weapon, ArsenalModificationPrice price);

        /// <summary>
        /// Attempts to pay for and install a modification.
        /// </summary>
        /// <remarks>
        /// Nothing is removed unless <see cref="CanInstall"/> holds.
        /// </remarks>
        /// <param name="weapon">The weapon item instance.</param>
        /// <param name="price">The price of the modification.</param>
        /// <returns>True if the modification was installed.</returns>
        bool TryInstall(ItemTable weapon, ArsenalModificationPrice price);
    }
}
