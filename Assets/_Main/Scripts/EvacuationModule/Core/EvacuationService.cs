using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using InputModule;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EvacuationModule
{
    public class EvacuationService : IDisposable, IEvacuationService
    {
        private readonly EvacuationConfig _config;

        private readonly IInputService _input;

        private CancellationTokenSource _tokenSource;

        public event Action Canceled;
        public event Action Completed;
        public event Action<float> TimerChanged;

        public EvacuationService(EvacuationConfig config,
            IInputService input)
        {
            _config = config;
            _input = input;
        }

        public void Dispose() => CancelTimer();

        public void ChangeZoneState(bool isInside)
        {
            if (isInside)
                StartTimer().Forget();
            else
                CancelTimer();
        }

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

                timer -= Time.deltaTime;
            }

            Complete();
        }

        private void Complete()
        {
            CancelTimer();

            Completed?.Invoke();

            if (_config.CompleteMode == EvacuationCompleteMode.LoadScene)
                SceneManager.LoadScene(_config.TargetSceneName);

            _input?.Disable();
        }
    }
}