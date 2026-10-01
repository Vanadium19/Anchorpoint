namespace TradeModule
{
    /// <summary>
    /// Exposes the trade offers of a world trader to <see cref="ITradeService"/>.
    /// </summary>
    /// <remarks>
    /// Implemented by <see cref="TraderView"/>. <see cref="TradeService"/> uses this to find the
    /// offers to display whenever it opens a UI belonging to a trader.
    /// </remarks>
    public interface ITraderOffersProvider
    {
        /// <summary>
        /// The offers configured for this trader.
        /// </summary>
        TraderOffersConfig Offers { get; }
    }
}
