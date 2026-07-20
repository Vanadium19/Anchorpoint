using System;
using System.Collections.Generic;

namespace BaseModule
{
    public interface IPauseManager
    {
        event Action<bool> PauseStateChanged;

        bool IsPaused { get; }
        IReadOnlyCollection<PauseReason> ActiveReasons { get; }

        bool HasReason(PauseReason reason);
        void Pause(PauseReason reason);
        void Resume(PauseReason reason);
        void ToggleUserPause();
        void Register(IPausable pausable);
        void Unregister(IPausable pausable);
        void Clear();
    }
}
