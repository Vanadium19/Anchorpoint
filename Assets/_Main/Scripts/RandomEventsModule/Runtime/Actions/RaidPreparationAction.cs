using System.Threading;
using ComponentsModule;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace RandomEventsModule
{
    public sealed class RaidPreparationAction : RandomEventActionBase
    {
        private readonly IRandomEventClock _clock;
        private readonly IRandomEventNotifier _notifier;
        private readonly IRaidPreparationProvider _preparationProvider;

        public RaidPreparationAction(
            IRandomEventClock clock,
            IRandomEventNotifier notifier,
            [InjectOptional] IRaidPreparationProvider preparationProvider)
        {
            _clock = clock;
            _notifier = notifier;
            _preparationProvider = preparationProvider;
        }

        public override async UniTask<bool> ExecuteAsync(CancellationToken token)
        {
            if (_preparationProvider == null ||
                !_preparationProvider.TryGetPreparation(out var preparationInfo))
            {
                return true;
            }

            var preparationSeconds = Mathf.Max(preparationInfo.PreparationSeconds, 0f);

            if (!string.IsNullOrEmpty(preparationInfo.NotificationLocalizationKey))
            {
                _notifier.ShowFormatted(
                    preparationInfo.NotificationLocalizationKey,
                    preparationInfo.NotificationDurationSeconds,
                    Mathf.CeilToInt(preparationSeconds));
            }

            if (preparationSeconds <= 0f)
                return true;

            var canceled = await _clock.DelayAsync(preparationSeconds, token);

            return !canceled;
        }
    }
}
