namespace ArsenalModule
{
    /// <summary>
    /// Exposes the price configuration of a world Arsenal to <see cref="IArsenalService"/>.
    /// </summary>
    /// <remarks>
    /// Implemented by <see cref="ArsenalStationView"/>. <see cref="ArsenalService"/> uses this to find
    /// the config to display whenever it opens a UI belonging to an Arsenal.
    /// </remarks>
    public interface IArsenalStation
    {
        /// <summary>
        /// The price configuration of this Arsenal.
        /// </summary>
        ArsenalConfig Config { get; }
    }
}
