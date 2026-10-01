using System.Threading;
using Cysharp.Threading.Tasks;

namespace RandomEventsModule
{
    /// <summary>The module's own clock: it only counts time the game was not paused on.</summary>
    /// <remarks>
    /// Everything an event measures goes through it — waits, cooldowns, the intervals inside long
    /// actions — so a fire that spreads every 10 seconds does not spread while the game is paused.
    /// </remarks>
    public interface IRandomEventClock
    {
        /// <summary>Whether the game is paused right now.</summary>
        bool IsPaused { get; }

        /// <summary>Seconds of unpaused time since the scene started.</summary>
        float ElapsedSeconds { get; }

        /// <summary>Waits the given number of seconds of unpaused time; returns <c>true</c> if the wait was canceled.</summary>
        UniTask<bool> DelayAsync(float seconds, CancellationToken token);
    }
}
