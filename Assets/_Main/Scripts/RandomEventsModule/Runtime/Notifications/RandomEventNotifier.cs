using System;
using UtilsModule;

namespace RandomEventsModule
{
    /// <summary>Resolves localization keys and raises <see cref="MessageRequested"/> for the presenter to show.</summary>
    public class RandomEventNotifier : IRandomEventNotifier
    {
        /// <inheritdoc/>
        public event Action<RandomEventNotificationMessage> MessageRequested;

        /// <inheritdoc/>
        public void Show(string localizationKey, float durationSeconds)
        {
            if (string.IsNullOrEmpty(localizationKey))
                return;

            MessageRequested?.Invoke(new RandomEventNotificationMessage(LocalizedText.Get(localizationKey), durationSeconds));
        }

        /// <inheritdoc/>
        public void ShowFormatted(string localizationKey, float durationSeconds, params object[] args)
        {
            if (string.IsNullOrEmpty(localizationKey))
                return;

            MessageRequested?.Invoke(new RandomEventNotificationMessage(LocalizedText.GetFormatted(localizationKey, args), durationSeconds));
        }
    }
}
