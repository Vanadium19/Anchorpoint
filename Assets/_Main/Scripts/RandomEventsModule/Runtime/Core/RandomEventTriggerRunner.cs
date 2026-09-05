using System;
using System.Collections.Generic;
using System.Threading;
using BaseModule;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>Default <see cref="IRandomEventTriggerRunner"/>: one pausable loop that polls every trigger of the scope on its own interval.</summary>
    /// <remarks>Triggers are resolved once at initialization, so a source, chance or picker can keep state between polls.</remarks>
    public class RandomEventTriggerRunner : IRandomEventTriggerRunner, IInitializable, IDisposable, IPausable
    {
        private readonly IRandomEventService _service;
        private readonly IRandomEventStateStore _store;
        private readonly RandomEventScope _scope;
        private readonly IPauseManager _pauseManager;
        private readonly DiContainer _container;
        private readonly List<RandomEventTriggerInstance> _triggers = new();

        private CancellationTokenSource _tokenSource;
        private bool _isPaused;

        /// <summary>Creates the runner bound to one scene's service, store, scope and container.</summary>
        public RandomEventTriggerRunner(
            IRandomEventService service,
            IRandomEventStateStore store,
            RandomEventScope scope,
            IPauseManager pauseManager,
            DiContainer container)
        {
            _service = service;
            _store = store;
            _scope = scope;
            _pauseManager = pauseManager;
            _container = container;
        }

        /// <summary>Resolves the scope's triggers, registers with the pause manager and starts polling.</summary>
        public void Initialize()
        {
            CreateTriggers();
            _pauseManager.Register(this);
            StartRunning();
        }

        /// <inheritdoc/>
        public void StartRunning()
        {
            StopRunning();

            _tokenSource = new CancellationTokenSource();

            RunLoopAsync(_tokenSource.Token).Forget();
        }

        /// <inheritdoc/>
        public void StopRunning()
        {
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
            _tokenSource = null;
        }

        /// <inheritdoc/>
        public RandomEventStartResult ReportSignal(RandomEventKey signal, int amount = 1)
        {
            _store.AddInt(signal, amount);

            return PollTriggers(trigger => trigger.HandlesSignal(signal));
        }

        /// <inheritdoc/>
        public RandomEventStartResult Evaluate() => PollTriggers(_ => true);

        /// <summary>Pauses or resumes polling without dropping the resolved triggers.</summary>
        public void SetPaused(bool isPaused) => _isPaused = isPaused;

        /// <summary>Unregisters from the pause manager and stops polling.</summary>
        public void Dispose()
        {
            _pauseManager.Unregister(this);
            StopRunning();
        }

        private void CreateTriggers()
        {
            foreach (var trigger in _scope.Triggers)
            {
                if (trigger == null || !trigger.IsValid)
                    continue;

                _triggers.Add(trigger.Create(_container));
            }
        }

        private RandomEventStartResult PollTriggers(Func<RandomEventTriggerInstance, bool> filter)
        {
            if (_isPaused)
                return RandomEventStartResult.Blocked;

            var result = RandomEventStartResult.NoCandidates;

            foreach (var trigger in _triggers)
            {
                if (!filter(trigger))
                    continue;

                var triggerResult = Fire(trigger);

                if (triggerResult == RandomEventStartResult.Started || result != RandomEventStartResult.Started)
                    result = triggerResult;
            }

            return result;
        }

        private RandomEventStartResult Fire(RandomEventTriggerInstance trigger)
        {
            try
            {
                if (!trigger.TryFire())
                    return RandomEventStartResult.NoCandidates;

                return _service.StartFromPool(trigger.Pool, trigger.Picker, _tokenSource?.Token ?? CancellationToken.None);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);

                return RandomEventStartResult.Blocked;
            }
        }

        private async UniTaskVoid RunLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var canceled = await UniTask.Yield(PlayerLoopTiming.Update, token).SuppressCancellationThrow();

                if (canceled)
                    return;

                if (_isPaused)
                    continue;

                var deltaTime = Time.deltaTime;

                PollTriggers(trigger => trigger.IsPollDue(deltaTime));
            }
        }
    }
}
