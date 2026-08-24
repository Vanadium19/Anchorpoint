using System;
using UtilsModule;
using Zenject;

namespace EvacuationModule
{
    public class EvacuationPresenter : IInitializable, IDisposable
    {
        private const string TimerFormatKey = "evac_timer_format";

        private readonly IEvacuationService _evacuationService;
        private readonly EvacuationView _view;

        public EvacuationPresenter(IEvacuationService evacuationService, EvacuationView view)
        {
            _evacuationService = evacuationService;
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
            var text = LocalizedText.GetFormatted(TimerFormatKey, time);
            _view.ShowTimer(text);
        }
    }
}