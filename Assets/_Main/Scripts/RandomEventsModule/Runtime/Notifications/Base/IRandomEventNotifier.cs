using System;

namespace RandomEventsModule
{
    /// <summary>Requests a localized, timed message be shown on the HUD.</summary>
    public interface IRandomEventNotifier
    {
        /// <summary>Raised whenever a message should be displayed.</summary>
        event Action<RandomEventNotificationMessage> MessageRequested;

        /// <summary>Shows the localized text for the given key.</summary>
        void Show(string localizationKey, float durationSeconds);

        /// <summary>Shows the localized, formatted text for the given key.</summary>
        void ShowFormatted(string localizationKey, float durationSeconds, params object[] args);
    }
}
