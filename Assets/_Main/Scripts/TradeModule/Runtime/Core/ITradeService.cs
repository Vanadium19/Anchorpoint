using System;

namespace TradeModule
{
    /// <summary>
    /// Checks and executes trades against the player's inventory.
    /// </summary>
    public interface ITradeService
    {
        /// <summary>
        /// Raised after a trade has been completed and the inventory has changed.
        /// </summary>
        event Action TradeStateChanged;

        /// <summary>
        /// Checks whether the player currently holds every item <paramref name="offer"/> requests.
        /// </summary>
        /// <param name="offer">The offer to check.</param>
        /// <returns>True if the trade can be completed as-is.</returns>
        bool CanTrade(TradeOfferConfig offer);

        /// <summary>
        /// Attempts to complete a trade: removes every requested item and grants every offered item.
        /// </summary>
        /// <remarks>
        /// The operation is atomic. If the player does not hold everything <paramref name="offer"/>
        /// requests, nothing is removed or granted and the method returns false.
        /// </remarks>
        /// <param name="offer">The offer to complete.</param>
        /// <returns>True if the trade was completed.</returns>
        bool TryTrade(TradeOfferConfig offer);
    }
}
