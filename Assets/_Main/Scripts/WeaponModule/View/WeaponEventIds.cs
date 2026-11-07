namespace WeaponModule
{
    /// <summary>
    /// Ids of the weapon moments that parts react to. State ids are raised by WeaponView,
    /// the rest are raised by the RaiseEvent Animation Event of clips.
    /// </summary>
    public static class WeaponEventIds
    {
        public const string Fired = "Fired";
        public const string ReloadStarted = "ReloadStarted";
        public const string EmptyReloadStarted = "EmptyReloadStarted";
        public const string RoundInserted = "RoundInserted";
        public const string ActionCycleCompleted = "ActionCycleCompleted";
        public const string RoundsLoaded = "RoundsLoaded";
    }
}
