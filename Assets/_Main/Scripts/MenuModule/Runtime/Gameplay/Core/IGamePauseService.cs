using System;
using System.Collections.Generic;

namespace MenuModule
{
    public interface IGamePauseService
    {
        event Action<bool> PauseStateChanged;

        bool IsPaused { get; }
        IReadOnlyCollection<GamePauseReason> ActiveReasons { get; }

        bool HasReason(GamePauseReason reason);
        void Pause(GamePauseReason reason);
        void Resume(GamePauseReason reason);
        void ToggleUserPause();

        void Clear();
    }
}