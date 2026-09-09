using System;
using System.Threading;
using BaseModule;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>Default <see cref="IRandomEventClock"/>: counts down in <see cref="Time.deltaTime"/> and stops counting while the game is paused.</summary>
    public class RandomEventClock : IRandomEventClock, IPausable, IInitializable, IDisposable
    {
        private readonly IPauseManager _pauseManager;

        private bool _isPaused;

        /// <summary>Creates the clock bound to the scene's pause manager.</summary>
        public RandomEventClock(IPauseManager pauseManager)
        {
            _pauseManager = pauseManager;
        }

        /// <summary>Registers with the pause manager.</summary>
        public void Initialize() => _pauseManager.Register(this);

        /// <summary>Unregisters from the pause manager.</summary>
        public void Dispose() => _pauseManager.Unregister(this);

        /// <summary>Pauses or resumes every wait handed out by this clock.</summary>
        public void SetPaused(bool isPaused) => _isPaused = isPaused;

        /// <inheritdoc/>
        public async UniTask<bool> DelayAsync(float seconds, CancellationToken token)
        {
            var remainingTime = seconds;

            do
            {
                var canceled = await UniTask.Yield(PlayerLoopTiming.Update, token).SuppressCancellationThrow();

                if (canceled)
                    return true;

                if (_isPaused)
                    continue;

                remainingTime -= Time.deltaTime;
            }
            while (remainingTime > 0f);

            return false;
        }
    }
}
