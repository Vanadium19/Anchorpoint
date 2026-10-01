namespace BaseModule
{
    public interface ICraftSpeedSource
    {
        /// <summary>
        /// Multiplier applied to crafting time progress of this workbench.
        /// </summary>
        float CraftSpeedMultiplier { get; }
    }
}
