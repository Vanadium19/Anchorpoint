using System.Threading;
using Cysharp.Threading.Tasks;

namespace RandomEventsModule
{
    /// <summary>Pause-aware waiting for event actions.</summary>
    /// <remarks>Time spent paused does not count, so a fire that spreads every 10 seconds does not spread while the game is paused.</remarks>
    public interface IRandomEventClock
    {
        /// <summary>Waits the given number of seconds of unpaused time; returns <c>true</c> if the wait was canceled.</summary>
        UniTask<bool> DelayAsync(float seconds, CancellationToken token);
    }
}
