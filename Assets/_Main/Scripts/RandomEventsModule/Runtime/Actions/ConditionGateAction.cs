using System.Threading;
using Cysharp.Threading.Tasks;

namespace RandomEventsModule
{
    /// <summary>Evaluates a set of conditions and gates the rest of the sequence on them.</summary>
    /// <remarks>Returns <c>false</c> when the conditions are not met, which stops the rest of the sequence and leaves the event non-occurred, exactly like <see cref="ChanceGateAction"/> but decided by state instead of a roll. An empty condition set is always met.</remarks>
    public class ConditionGateAction : RandomEventActionBase
    {
        private readonly RandomEventConditionGroup _conditions;

        /// <summary>Creates the gate over the given conditions.</summary>
        public ConditionGateAction(RandomEventConditionGroup conditions)
        {
            _conditions = conditions;
        }

        /// <inheritdoc/>
        public override UniTask<bool> ExecuteAsync(CancellationToken token) =>
            UniTask.FromResult(_conditions == null || _conditions.IsMet());
    }
}
