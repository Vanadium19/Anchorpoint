using System.Threading;
using Cysharp.Threading.Tasks;

namespace RandomEventsModule
{
    /// <summary>Convenience base for actions that don't need to react to <see cref="RequestStop"/>.</summary>
    public abstract class RandomEventActionBase : IRandomEventAction
    {
        /// <inheritdoc/>
        public abstract UniTask<bool> ExecuteAsync(CancellationToken token);

        /// <inheritdoc/>
        public virtual void RequestStop()
        {
        }
    }
}
