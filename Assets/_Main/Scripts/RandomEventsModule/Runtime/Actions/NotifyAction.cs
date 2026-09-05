using System.Threading;
using Cysharp.Threading.Tasks;

namespace RandomEventsModule
{
    /// <summary>Shows one localized HUD message and continues immediately.</summary>
    public class NotifyAction : RandomEventActionBase
    {
        private readonly IRandomEventNotifier _notifier;
        private readonly string _localizationKey;
        private readonly float _durationSeconds;

        /// <summary>Creates the action over the scene notifier.</summary>
        public NotifyAction(IRandomEventNotifier notifier, string localizationKey, float durationSeconds)
        {
            _notifier = notifier;
            _localizationKey = localizationKey;
            _durationSeconds = durationSeconds;
        }

        /// <inheritdoc/>
        public override UniTask<bool> ExecuteAsync(CancellationToken token)
        {
            _notifier.Show(_localizationKey, _durationSeconds);

            return UniTask.FromResult(true);
        }
    }
}
