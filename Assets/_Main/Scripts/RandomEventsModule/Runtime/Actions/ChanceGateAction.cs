using System.Threading;
using Cysharp.Threading.Tasks;

namespace RandomEventsModule
{
    /// <summary>Rolls a chance and gates the rest of the sequence on it.</summary>
    public class ChanceGateAction : RandomEventActionBase
    {
        private readonly IRandomEventChance _chance;

        /// <summary>Creates the gate over the given chance.</summary>
        public ChanceGateAction(IRandomEventChance chance)
        {
            _chance = chance;
        }

        /// <inheritdoc/>
        public override UniTask<bool> ExecuteAsync(CancellationToken token) => UniTask.FromResult(_chance == null || _chance.Roll());
    }
}
