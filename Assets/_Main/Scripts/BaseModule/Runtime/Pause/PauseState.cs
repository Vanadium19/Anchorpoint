using System;

namespace BaseModule
{
    public static class PauseState
    {
        public static event Action<bool> PauseChanged;

        public static bool IsPaused { get; private set; }

        internal static void SetPaused(bool isPaused)
        {
            if (IsPaused == isPaused)
                return;

            IsPaused = isPaused;
            PauseChanged?.Invoke(isPaused);
        }
    }
}
