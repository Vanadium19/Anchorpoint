using System;
using System.Threading;
using BaseModule;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>Default <see cref="IRandomEventTriggerRunner"/>: a pausable, cancellable periodic loop plus on-demand evaluation.</summary>
    public class RandomEventTriggerRunner : IRandomEventTriggerRunner, IInitializable, IDisposable, IPausable
    {
        private readonly IRandomEventService _service;
        private readonly IRandomEventStateStore _store;
        private readonly RandomEventsRules _rules;
        private readonly RandomEventScope _scope;
        private readonly IPauseManager _pauseManager;

        private CancellationTokenSource _tokenSource;
        private bool _isPaused;

        /// <summary>Creates the runner bound to one scene's service, store, rules and scope.</summary>
        public RandomEventTriggerRunner(
            IRandomEventService service,
            IRandomEventStateStore store,
            RandomEventsRules rules,
            RandomEventScope scope,
            IPauseManager pauseManager)
        {
            _service = service;
            _store = store;
            _rules = rules;
            _scope = scope;
            _pauseManager = pauseManager;
        }

        /// <summary>Registers with the pause manager and starts the periodic loop.</summary>
        public void Initialize()
        {
            _pauseManager.Register(this);
            StartRunning();
        }

        /// <inheritdoc/>
        public void StartRunning()
        {
            StopRunning();

            _tokenSource = new CancellationTokenSource();

            if (_rules.EvaluationIntervalSeconds > 0f)
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
        public void ReportSignal(RandomEventKey signal, int amount = 1)
        {
            _store.AddInt(signal, amount);
            Evaluate();
        }

        /// <inheritdoc/>
        public RandomEventStartResult Evaluate()
        {
            if (_isPaused)
                return RandomEventStartResult.Blocked;

            try
            {
                return _service.StartRandom(_scope, _tokenSource?.Token ?? CancellationToken.None);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return RandomEventStartResult.Blocked;
            }
        }

        /// <summary>Pauses or resumes evaluation without stopping the loop.</summary>
        public void SetPaused(bool isPaused) => _isPaused = isPaused;

        /// <summary>Unregisters from the pause manager and stops the loop.</summary>
        public void Dispose()
        {
            _pauseManager.Unregister(this);
            StopRunning();
        }

        private async UniTaskVoid RunLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var canceled = await WaitEvaluationIntervalAsync(token);

                if (canceled)
                    return;

                Evaluate();
            }
        }

        private async UniTask<bool> WaitEvaluationIntervalAsync(CancellationToken token)
        {
            var remainingTime = _rules.EvaluationIntervalSeconds;

            while (remainingTime > 0f)
            {
                var canceled = await UniTask.Yield(PlayerLoopTiming.Update, token)
                    .SuppressCancellationThrow();

                if (canceled)
                    return true;

                if (_isPaused)
                    continue;

                remainingTime -= Time.deltaTime;
            }

            return false;
        }
    }
}
