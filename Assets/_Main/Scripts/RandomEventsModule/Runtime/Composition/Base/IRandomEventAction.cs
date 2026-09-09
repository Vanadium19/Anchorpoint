using System.Threading;
using Cysharp.Threading.Tasks;

namespace RandomEventsModule
{
    /// <summary>One runtime step of an event's action sequence.</summary>
    public interface IRandomEventAction
    {
        /// <summary>Runs the action; return <c>false</c> to act as a gate and stop the remaining sequence.</summary>
        UniTask<bool> ExecuteAsync(CancellationToken token);

        /// <summary>Requests an early, cooperative stop while <see cref="ExecuteAsync"/> is running.</summary>
        void RequestStop();
    }
}
