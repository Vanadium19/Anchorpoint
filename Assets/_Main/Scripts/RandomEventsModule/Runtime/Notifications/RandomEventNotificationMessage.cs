namespace RandomEventsModule
{
    /// <summary>A resolved message text and how long it should stay on screen.</summary>
    public readonly struct RandomEventNotificationMessage
    {
        /// <summary>Creates a message.</summary>
        public RandomEventNotificationMessage(string text, float durationSeconds)
        {
            Text = text;
            DurationSeconds = durationSeconds;
        }

        /// <summary>The already-localized text to show.</summary>
        public string Text { get; }

        /// <summary>How long to keep the message visible, in seconds.</summary>
        public float DurationSeconds { get; }
    }
}
