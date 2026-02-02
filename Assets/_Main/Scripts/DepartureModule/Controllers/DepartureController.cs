using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace DepartureModule
{
    public class DepartureController : IInitializable, IDisposable
    {
        private readonly DepartureConfig _config;
        private readonly DepartureZoneView _zoneView;
        private readonly DepartureHUDView _hudView;

        private CancellationTokenSource _cts;

        public DepartureController(DepartureConfig config,
            DepartureZoneView zoneView,
            DepartureHUDView hudView)
        {
            _config = config;
            _zoneView = zoneView;
            _hudView = hudView;
        }

        public void Initialize()
        {
            _hudView.Hide();
            _zoneView.PlayerPresenceChanged += OnZoneStateChanged;
        }

        public void Dispose()
        {
            _zoneView.PlayerPresenceChanged -= OnZoneStateChanged;
            CancelTimer();
        }

        private void OnZoneStateChanged(bool isInside)
        {
            if (isInside) StartTimer().Forget();
            else CancelTimer();
        }

        private async UniTaskVoid StartTimer()
        {
            CancelTimer();
            _cts = new CancellationTokenSource();

            float timer = _config.DepartureTime;

            try
            {
                while (timer > 0)
                {
                    _hudView.ShowTimer(timer, _config.TimerTextFormat);
                    await UniTask.Yield(PlayerLoopTiming.Update, _cts.Token);
                    timer -= Time.deltaTime;
                }

                CompleteDeparture();
            }
            catch (OperationCanceledException)
            {
                _hudView.Hide();
            }
        }

        private void CancelTimer()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            if (_hudView != null)
            {
                _hudView.Hide();
            }
        }

        private void CompleteDeparture()
        {
            SceneManager.LoadScene(_config.TargetSceneName);
        }
    }
}