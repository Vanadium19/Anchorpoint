using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using InputModule;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace EvacuationModule
{
    public class EvacuationController : IInitializable, IDisposable
    {
        private readonly EvacuationConfig _config;
        private readonly EvacuationZoneView _zoneView;
        private readonly EvacuationHUDView _hudView;

        private readonly IInputService _input;

        private CancellationTokenSource _cts;

        public EvacuationController(
            EvacuationConfig config,
            EvacuationZoneView zoneView,
            EvacuationHUDView hudView,
            [InjectOptional] IInputService input)
        {
            _config = config;
            _zoneView = zoneView;
            _hudView = hudView;
            _input = input;
        }

        public void Initialize()
        {
            if (_hudView != null)
            {
                _hudView.HideTimer();
                _hudView.Setup(_config.BaseSceneName);
            }

            if (_zoneView != null)
                _zoneView.PlayerPresenceChanged += OnZoneStateChanged;
        }

        public void Dispose()
        {
            if (_zoneView != null)
                _zoneView.PlayerPresenceChanged -= OnZoneStateChanged;

            CancelTimer();
        }

        private void OnZoneStateChanged(bool isInside)
        {
            if (isInside)
                StartTimer().Forget();
            else
                CancelTimer();
        }

        private void CancelTimer()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            if (_hudView != null && !_hudView.Equals(null))
                _hudView.HideTimer();
        }

        private async UniTaskVoid StartTimer()
        {
            CancelTimer();
            _cts = new CancellationTokenSource();

            var token = _cts.Token;
            float timer = _config.Duration;

            while (timer > 0f)
            {
                if (_hudView == null || _hudView.Equals(null))
                    return;

                _hudView.ShowTimer(timer, _config.TimerTextFormat);

                bool canceled = await UniTask.Yield(PlayerLoopTiming.Update, token)
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

            if (_config.CompleteMode == EvacuationCompleteMode.LoadScene)
            {
                SceneManager.LoadScene(_config.TargetSceneName);
                return;
            }

            if (_hudView != null && !_hudView.Equals(null))
                _hudView.ShowSuccessScreen();

            _input?.Disable();
        }
    }
}