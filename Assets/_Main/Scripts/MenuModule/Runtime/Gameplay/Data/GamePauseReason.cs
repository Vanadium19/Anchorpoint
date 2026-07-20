using System;

namespace MenuModule
{
    [Obsolete("Use PauseReason instead.")]
    public enum GamePauseReason
    {
        UserPause = 0,
        Victory = 1,
    }
}
