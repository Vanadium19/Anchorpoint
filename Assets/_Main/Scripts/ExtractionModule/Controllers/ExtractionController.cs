using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Zenject;
using UnityEngine;
using InputModule;

namespace ExtractionModule
{
    public class ExtractionController : IInitializable, IDisposable
    {
        private readonly ExtractionConfig _config;
        private readonly ExtractionZoneView _zoneView;
        private readonly ExtractionHUDView _hudView;
        private readonly IInputService _input;

        private CancellationTokenSource _tokenSource;

        public ExtractionController(ExtractionConfig config,
            ExtractionZoneView zoneView,
            ExtractionHUDView hudView,
            IInputService input)
        {
            _config = config;
            _zoneView = zoneView;
            _hudView = hudView;
            _input = input;
        }

        public void Initialize()
        {
            if (_zoneView != null)
                _zoneView.PlayerPresenceChanged += OnZoneStateChanged;
        }

        public void Dispose()
        {
            if (_zoneView != null)
                _zoneView.PlayerPresenceChanged -= OnZoneStateChanged;

            CancelExtraction();
        }

        private void OnZoneStateChanged(bool isInside)
        {
            if (isInside)
                StartExtraction().Forget();
            else
                CancelExtraction();
        }

        private void CancelExtraction()
        {
            if (_tokenSource != null)
            {
                _tokenSource.Cancel();
                _tokenSource.Dispose();
                _tokenSource = null;
            }

            if (_hudView && !_hudView.Equals(null))
                _hudView.HideTimer();
        }

        private async UniTaskVoid StartExtraction()
        {
            CancelExtraction();
            _tokenSource = new();

            var token = _tokenSource.Token;
            var timer = _config.ExtractionTime;

            while (timer > 0)
            {
                if (_hudView == null || _hudView.Equals(null))
                    return;

                _hudView.ShowTimer(timer, _config.TimerTextFormat);
                bool isCanceled = await UniTask.Yield(PlayerLoopTiming.Update, token).SuppressCancellationThrow();

                if (isCanceled)
                    return;

                timer -= Time.deltaTime;
            }

            CompleteExtraction();
        }

        private void CompleteExtraction()
        {
            if (_tokenSource != null)
            {
                _tokenSource.Dispose();
                _tokenSource = null;
            }

            if (_hudView != null)
            {
                _hudView.HideTimer();
                _hudView.ShowSuccessScreen();
            }

            _input?.Disable();
        }
    }
}