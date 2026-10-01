using System.Threading;
using Cysharp.Threading.Tasks;

namespace RandomEventsModule
{
    /// <summary>Waits the configured number of seconds before the sequence continues.</summary>
    /// <remarks>The wait runs on <see cref="IRandomEventClock"/>, so time spent paused does not count.</remarks>
    public class DelayAction : RandomEventActionBase
    {
        private readonly IRandomEventClock _clock;
        private readonly float _durationSeconds;

        /// <summary>Creates the action with the pause it holds.</summary>
        public DelayAction(IRandomEventClock clock, float durationSeconds)
        {
            _clock = clock;
            _durationSeconds = durationSeconds;
        }

        /// <inheritdoc/>
        public override async UniTask<bool> ExecuteAsync(CancellationToken token)
        {
            await _clock.DelayAsync(_durationSeconds, token);

            return true;
        }
    }
}
