using System.Threading;
using Cysharp.Threading.Tasks;

namespace RandomEventsModule
{
    /// <summary>Lifts the hold taken by a <see cref="BlockEvacuationAction"/> with the same key.</summary>
    /// <remarks>An empty key lifts every hold, which is what a cleanup step at the end of an event usually wants.</remarks>
    public class UnblockEvacuationAction : RandomEventActionBase
    {
        private readonly IEvacuationBlocker _blocker;
        private readonly string _blockedMessageKey;

        /// <summary>Creates the action over the key it lifts.</summary>
        public UnblockEvacuationAction(IEvacuationBlocker blocker, string blockedMessageKey)
        {
            _blocker = blocker;
            _blockedMessageKey = blockedMessageKey;
        }

        /// <inheritdoc/>
        public override UniTask<bool> ExecuteAsync(CancellationToken token)
        {
            _blocker.Unblock(_blockedMessageKey);

            return UniTask.FromResult(true);
        }
    }
}
