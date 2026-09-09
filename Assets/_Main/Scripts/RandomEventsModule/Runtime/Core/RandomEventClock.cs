using System;
using System.Threading;
using BaseModule;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>Default <see cref="IRandomEventClock"/>: counts down in <see cref="Time.deltaTime"/> and stops counting while the game is paused.</summary>
    /// <remarks><see cref="ElapsedSeconds"/> is advanced by a loop of its own, so it stays the module's single answer to "how much time has really passed".</remarks>
    public class RandomEventClock : IRandomEventClock, IPausable, IInitializable, IDisposable
    {
        private readonly IPauseManager _pauseManager;

        private CancellationTokenSource _tokenSource;
        private bool _isPaused;
        private float _elapsedSeconds;

        /// <summary>Creates the clock bound to the scene's pause manager.</summary>
        public RandomEventClock(IPauseManager pauseManager)
        {
            _pauseManager = pauseManager;
        }

        /// <inheritdoc/>
        public bool IsPaused => _isPaused;

        /// <inheritdoc/>
        public float ElapsedSeconds => _elapsedSeconds;

        /// <summary>Registers with the pause manager and starts counting.</summary>
        public void Initialize()
        {
            _pauseManager.Register(this);
            _tokenSource = new CancellationTokenSource();

            RunAsync(_tokenSource.Token).Forget();
        }

        /// <summary>Unregisters from the pause manager and stops counting.</summary>
        public void Dispose()
        {
            _pauseManager.Unregister(this);
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
            _tokenSource = null;
        }

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

        private async UniTaskVoid RunAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var canceled = await UniTask.Yield(PlayerLoopTiming.Update, token).SuppressCancellationThrow();

                if (canceled)
                    return;

                if (_isPaused)
                    continue;

                _elapsedSeconds += Time.deltaTime;
            }
        }
    }
}
