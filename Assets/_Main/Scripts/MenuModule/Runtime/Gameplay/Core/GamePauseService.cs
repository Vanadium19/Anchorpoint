using System;
using System.Collections.Generic;
using UnityEngine;

namespace MenuModule
{
    public class GamePauseService : IGamePauseService, IDisposable
    {
        private readonly HashSet<GamePauseReason> _activeReasons = new();
        private float _timeScaleBeforePause = 1f;

        public event Action<bool> PauseStateChanged;

        public bool IsPaused => _activeReasons.Count > 0;
        public IReadOnlyCollection<GamePauseReason> ActiveReasons => _activeReasons;

        public bool HasReason(GamePauseReason reason) => _activeReasons.Contains(reason);

        public void Pause(GamePauseReason reason)
        {
            if (_activeReasons.Contains(reason))
                return;

            var wasPaused = IsPaused;

            if (!wasPaused)
                _timeScaleBeforePause = Time.timeScale;

            _activeReasons.Add(reason);
            Time.timeScale = 0f;

            if (!wasPaused)
                PauseStateChanged?.Invoke(true);
        }

        public void Resume(GamePauseReason reason)
        {
            if (!_activeReasons.Remove(reason))
                return;

            if (IsPaused)
                return;

            Time.timeScale = _timeScaleBeforePause;
            PauseStateChanged?.Invoke(false);
        }

        public void ToggleUserPause()
        {
            if (HasReason(GamePauseReason.UserPause))
                Resume(GamePauseReason.UserPause);
            else
                Pause(GamePauseReason.UserPause);
        }

        public void Dispose()
        {
            if (!IsPaused)
                return;

            _activeReasons.Clear();
            Time.timeScale = _timeScaleBeforePause;
        }
    }
}