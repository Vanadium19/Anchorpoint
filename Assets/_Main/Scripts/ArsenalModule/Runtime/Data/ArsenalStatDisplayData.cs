namespace ArsenalModule
{
    /// <summary>
    /// Precomputed display data for one weapon stat line.
    /// </summary>
    public class ArsenalStatDisplayData
    {
        /// <summary>
        /// Localized stat label.
        /// </summary>
        public string LabelText { get; set; }

        /// <summary>
        /// Formatted stat value of the weapon as it is now.
        /// </summary>
        public string CurrentValueText { get; set; }

        /// <summary>
        /// Formatted stat value after installing the selected modification.
        /// </summary>
        public string PreviewValueText { get; set; }

        /// <summary>
        /// Whether the preview value differs from the current one and should be shown.
        /// </summary>
        public bool HasPreview { get; set; }

        /// <summary>
        /// Whether the preview value is better for the player than the current one.
        /// </summary>
        public bool IsImprovement { get; set; }
    }
}
