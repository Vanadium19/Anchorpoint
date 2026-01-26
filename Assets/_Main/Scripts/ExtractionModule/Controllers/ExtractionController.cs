using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Zenject;
using UnityEngine;
using ExtractionModule.Configs;
using ExtractionModule.View;
using InputModule.Core;

namespace ExtractionModule.Controllers
{
    public class ExtractionController : IInitializable, IDisposable
    {
        private readonly ExtractionConfig _config;
        private readonly ExtractionZoneView _zoneView;
        private readonly ExtractionHUDView _hudView;
        private readonly IInputMap _input;

        private CancellationTokenSource _cts;

        public ExtractionController(
            ExtractionConfig config,
            ExtractionZoneView zoneView,
            ExtractionHUDView hudView,
            IInputMap input)
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
            {
                StartExtraction().Forget();
            }
            else
            {
                CancelExtraction();
            }
        }

        private void CancelExtraction()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
            if (_hudView != null && !_hudView.Equals(null))
            {
                _hudView.HideTimer();
            }
        }

        private async UniTaskVoid StartExtraction()
        {
            CancelExtraction();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            float timer = _config.ExtractionTime;

            while (timer > 0)
            {
                if (_hudView == null || _hudView.Equals(null)) return;
                _hudView.ShowTimer(timer, _config.TimerTextFormat);
                bool isCanceled = await UniTask.Yield(PlayerLoopTiming.Update, token).SuppressCancellationThrow();
                if (isCanceled) return;
                timer -= Time.deltaTime;
            }

            CompleteExtraction();
        }

        private void CompleteExtraction()
        {
            if (_cts != null)
            {
                _cts.Dispose();
                _cts = null;
            }

            if (_hudView != null)
            {
                _hudView.HideTimer();
                _hudView.ShowSuccessScreen();
            }

            if (_input != null)
            {
                _input.Disable();
            }
        }
    }
}