using System.Threading;
using Cysharp.Threading.Tasks;

namespace RandomEventsModule
{
    /// <summary>Forbids leaving the scene until an <see cref="UnblockEvacuationAction"/> with the same key runs.</summary>
    /// <remarks>The step finishes at once, so it is put before the part of the event that must not be skipped by evacuating.</remarks>
    public class BlockEvacuationAction : RandomEventActionBase
    {
        private readonly IEvacuationBlocker _blocker;
        private readonly string _blockedMessageKey;
        private readonly float _messageDurationSeconds;

        /// <summary>Creates the action with the message shown to a blocked player.</summary>
        public BlockEvacuationAction(IEvacuationBlocker blocker, string blockedMessageKey, float messageDurationSeconds)
        {
            _blocker = blocker;
            _blockedMessageKey = blockedMessageKey;
            _messageDurationSeconds = messageDurationSeconds;
        }

        /// <inheritdoc/>
        public override UniTask<bool> ExecuteAsync(CancellationToken token)
        {
            _blocker.Block(_blockedMessageKey, _messageDurationSeconds);

            return UniTask.FromResult(true);
        }
    }
}
