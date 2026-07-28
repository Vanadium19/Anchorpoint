using System;
using System.Collections.Generic;

namespace BaseModule
{
    public class PauseManager : IPauseManager
    {
        private readonly HashSet<PauseReason> _activeReasons = new();
        private readonly List<IPausable> _pausables = new();

        public event Action<bool> PauseStateChanged;

        public bool IsPaused => _activeReasons.Count > 0;
        public IReadOnlyCollection<PauseReason> ActiveReasons => _activeReasons;

        public bool HasReason(PauseReason reason) => _activeReasons.Contains(reason);

        public void Pause(PauseReason reason)
        {
            if (!_activeReasons.Add(reason))
                return;

            if (_activeReasons.Count == 1)
                ApplyPaused(true);
        }

        public void Resume(PauseReason reason)
        {
            if (!_activeReasons.Remove(reason))
                return;

            if (_activeReasons.Count == 0)
                ApplyPaused(false);
        }

        public void ToggleUserPause()
        {
            if (HasReason(PauseReason.UserPause))
                Resume(PauseReason.UserPause);
            else
                Pause(PauseReason.UserPause);
        }

        public void Register(IPausable pausable)
        {
            if (pausable == null || _pausables.Contains(pausable))
                return;

            _pausables.Add(pausable);
            pausable.SetPaused(IsPaused);
        }

        public void Unregister(IPausable pausable)
        {
            if (pausable == null)
                return;

            _pausables.Remove(pausable);
        }

        public void Clear()
        {
            if (_activeReasons.Count == 0)
                return;

            _activeReasons.Clear();
            ApplyPaused(false);
        }

        private void ApplyPaused(bool isPaused)
        {
            PauseState.SetPaused(isPaused);

            for (var i = _pausables.Count - 1; i >= 0; i--)
                _pausables[i]?.SetPaused(isPaused);

            PauseStateChanged?.Invoke(isPaused);
        }
    }
}
