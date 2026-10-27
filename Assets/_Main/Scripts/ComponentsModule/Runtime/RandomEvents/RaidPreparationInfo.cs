namespace ComponentsModule
{
    public readonly struct RaidPreparationInfo
    {
        public RaidPreparationInfo(
            float preparationSeconds,
            string notificationLocalizationKey,
            float notificationDurationSeconds)
        {
            PreparationSeconds = preparationSeconds;
            NotificationLocalizationKey = notificationLocalizationKey;
            NotificationDurationSeconds = notificationDurationSeconds;
        }

        public float PreparationSeconds { get; }
        public string NotificationLocalizationKey { get; }
        public float NotificationDurationSeconds { get; }
    }
}
