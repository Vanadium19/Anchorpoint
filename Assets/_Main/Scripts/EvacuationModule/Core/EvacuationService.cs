using System;
using System.Threading;
using BaseModule;
using Cysharp.Threading.Tasks;
using InputModule;
using SaveModule;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace EvacuationModule
{
    public class EvacuationService : IDisposable, IEvacuationService, IPausable
    {
        private readonly EvacuationConfig _config;
        private readonly IInputService _input;
        private readonly IGameSaveLoader _gameSaveLoader;
        private readonly IPauseManager _pauseManager;

        private CancellationTokenSource _tokenSource;
        private bool _isPaused;

        public event Action Canceled;
        public event Action Completed;
        public event Action<float> TimerChanged;

        public EvacuationService(
            EvacuationConfig config,
            IInputService input,
            IPauseManager pauseManager,
            [Inject(Id = GameSaveLoaderIds.Game)] IGameSaveLoader gameSaveLoader)
        {
            _config = config;
            _input = input;
            _pauseManager = pauseManager;
            _gameSaveLoader = gameSaveLoader;
            _pauseManager.Register(this);
        }

        public void Dispose()
        {
            _pauseManager.Unregister(this);
            CancelTimer();
        }

        public void ChangeZoneState(bool isInside)
        {
            if (isInside)
                StartTimer().Forget();
            else
                CancelTimer();
        }

        public void SetPaused(bool isPaused) => _isPaused = isPaused;

        private void CancelTimer()
        {
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
            _tokenSource = null;

            Canceled?.Invoke();
        }

        private async UniTaskVoid StartTimer()
        {
            CancelTimer();

            _tokenSource = new();

            var token = _tokenSource.Token;
            var timer = _config.Duration;

            while (timer > 0f)
            {
                TimerChanged?.Invoke(timer);

                var canceled = await UniTask.Yield(PlayerLoopTiming.Update, token)
                    .SuppressCancellationThrow();

                if (canceled)
                    return;

                if (_isPaused)
                    continue;

                timer -= Time.deltaTime;
            }

            Complete();
        }

        private void Complete()
        {
            CancelTimer();

            Completed?.Invoke();

            if (_config.CompleteMode == EvacuationCompleteMode.LoadScene)
            {
                _gameSaveLoader?.Save();
                SceneManager.LoadScene(_config.TargetSceneName);
            }

            _input?.Disable();
        }
    }
}
