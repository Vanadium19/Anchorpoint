using System;
using Zenject;

namespace EvacuationModule
{
    public class EvacuationPresenter : IInitializable, IDisposable
    {
        private readonly IEvacuationService _evacuationService;
        private readonly EvacuationConfig _config;
        private readonly EvacuationView _view;

        public EvacuationPresenter(IEvacuationService evacuationService, EvacuationView view, EvacuationConfig config)
        {
            _evacuationService = evacuationService;
            _config = config;
            _view = view;
        }

        public void Initialize()
        {
            _view.HideTimer();

            _evacuationService.Canceled += _view.HideTimer;
            _evacuationService.Completed += _view.ShowSuccessScreen;
            _evacuationService.TimerChanged += OnTimerChanged;
        }

        public void Dispose()
        {
            _evacuationService.Canceled -= _view.HideTimer;
            _evacuationService.Completed -= _view.ShowSuccessScreen;
            _evacuationService.TimerChanged -= OnTimerChanged;
        }

        private void OnTimerChanged(float time)
        {
            var text = string.Format(_config.TimerTextFormat, time);
            _view.ShowTimer(text);
        }
    }
}