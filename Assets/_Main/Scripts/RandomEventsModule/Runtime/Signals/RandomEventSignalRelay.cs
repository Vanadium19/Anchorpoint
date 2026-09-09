using System;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>Base for orchestration-layer adapters that turn a fact published by a game module into a random event signal.</summary>
    /// <remarks>
    /// The game module only exposes its own event and stays unaware of random events; the relay is
    /// the single place that knows about both sides. Derived relays bind their signal key with
    /// <c>WithArguments</c> in the orchestration installer.
    /// </remarks>
    public abstract class RandomEventSignalRelay : IInitializable, IDisposable
    {
        private readonly IRandomEventTriggerRunner _triggerRunner;
        private readonly string _signal;

        /// <summary>Creates the relay bound to a trigger runner and the signal key it reports under.</summary>
        protected RandomEventSignalRelay(IRandomEventTriggerRunner triggerRunner, string signal)
        {
            _triggerRunner = triggerRunner;
            _signal = signal;
        }

        /// <summary>Subscribes to the game module fact.</summary>
        public void Initialize() => Subscribe();

        /// <summary>Unsubscribes from the game module fact.</summary>
        public void Dispose() => Unsubscribe();

        /// <summary>Subscribes to the underlying game module event.</summary>
        protected abstract void Subscribe();

        /// <summary>Unsubscribes from the underlying game module event.</summary>
        protected abstract void Unsubscribe();

        /// <summary>Reports the relay signal to the trigger runner.</summary>
        protected void Report(int amount = 1) => _triggerRunner.ReportSignal(_signal, amount);
    }
}
