using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>
    /// Drives the lifecycle of random events: picking, starting, running and stopping them,
    /// while enforcing cooldowns, exclusivity and the minimum interval between events.
    /// The service has no knowledge of any concrete event, only of pools and action plans.
    /// </summary>
    /// <remarks>
    /// An event whose sequence was gated — a step returned <c>false</c> — counts as one that never
    /// happened: it takes no cooldown and does not move the minimum interval.
    /// </remarks>
    public class RandomEventService : IRandomEventService, IDisposable
    {
        private readonly RandomEventScope _scope;
        private readonly DiContainer _container;
        private readonly Dictionary<RandomEventDefinition, CancellationTokenSource> _activeTokenSources = new();
        private readonly Dictionary<RandomEventDefinition, RandomEventInfo> _activeEventInfos = new();
        private readonly Dictionary<RandomEventDefinition, RandomEventActionPlan> _activePlans = new();
        private readonly Dictionary<RandomEventDefinition, float> _cooldownEndTimes = new();

        private float _lastEventEndTime = float.NegativeInfinity;

        /// <inheritdoc/>
        public event Action<RandomEventInfo> EventStarted;

        /// <inheritdoc/>
        public event Action<RandomEventInfo> EventFinished;

        /// <summary>Creates the service bound to one scene's scope and DI container.</summary>
        public RandomEventService(RandomEventScope scope, DiContainer container)
        {
            _scope = scope;
            _container = container;
        }

        /// <inheritdoc/>
        public bool IsAnyEventActive => _activeEventInfos.Count > 0;

        /// <inheritdoc/>
        public IReadOnlyList<RandomEventInfo> ActiveEvents => _activeEventInfos.Values.ToList();

        /// <inheritdoc/>
        public bool IsEventActive(RandomEventDefinition definition) => definition != null && _activeEventInfos.ContainsKey(definition);

        /// <inheritdoc/>
        public RandomEventStartResult Start(RandomEventDefinition definition, CancellationToken token) =>
            definition == null ? RandomEventStartResult.NotFound : StartDefinition(definition, token);

        /// <inheritdoc/>
        public RandomEventStartResult StartFromPool(RandomEventPool pool, IRandomEventPicker picker, CancellationToken token)
        {
            if (pool == null || picker == null)
                return RandomEventStartResult.NoCandidates;

            if (IsGloballyBlocked())
                return RandomEventStartResult.Blocked;

            var candidates = CreateCandidates(pool);

            if (candidates.Count == 0)
                return RandomEventStartResult.NoCandidates;

            var definition = picker.Pick(candidates);

            return definition == null
                ? RandomEventStartResult.NoCandidates
                : StartDefinition(definition, token);
        }

        /// <inheritdoc/>
        public void Stop(RandomEventDefinition definition)
        {
            if (definition == null || !_activeTokenSources.TryGetValue(definition, out var tokenSource))
                return;

            if (_activePlans.TryGetValue(definition, out var plan))
                plan.RequestStop();

            tokenSource.Cancel();
        }

        /// <inheritdoc/>
        public void StopAll()
        {
            foreach (var definition in _activeTokenSources.Keys.ToList())
                Stop(definition);
        }

        /// <inheritdoc/>
        public void Dispose() => StopAll();

        private List<RandomEventCandidate> CreateCandidates(RandomEventPool pool) =>
            pool.Entries
                .Where(entry => entry?.Definition != null && entry.Weight > 0f && CanStartDefinition(entry.Definition))
                .Select(entry => new RandomEventCandidate(entry.Definition, entry.Weight))
                .ToList();

        private RandomEventStartResult StartDefinition(RandomEventDefinition definition, CancellationToken token)
        {
            var blockReason = GetBlockReason(definition);

            if (blockReason.HasValue)
                return blockReason.Value;

            if (IsGloballyBlocked())
                return RandomEventStartResult.Blocked;

            var plan = definition.CreatePlan(_container);
            var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
            var info = definition.CreateInfo();

            _activeTokenSources[definition] = tokenSource;
            _activeEventInfos[definition] = info;
            _activePlans[definition] = plan;

            EventStarted?.Invoke(info);

            RunEventAsync(definition, info, plan, tokenSource.Token).Forget();

            return RandomEventStartResult.Started;
        }

        private bool CanStartDefinition(RandomEventDefinition definition) => GetBlockReason(definition) == null;

        private RandomEventStartResult? GetBlockReason(RandomEventDefinition definition)
        {
            if (!definition.IsEnabled)
                return RandomEventStartResult.Disabled;

            if (_activeEventInfos.ContainsKey(definition))
                return RandomEventStartResult.AlreadyActive;

            if (IsOnCooldown(definition))
                return RandomEventStartResult.Blocked;

            return null;
        }

        private bool IsOnCooldown(RandomEventDefinition definition) =>
            _cooldownEndTimes.TryGetValue(definition, out var cooldownEndTime) && Time.time < cooldownEndTime;

        private bool IsGloballyBlocked() =>
            IsExclusiveEventActive() || Time.time - _lastEventEndTime < _scope.MinIntervalBetweenEventsSeconds;

        private bool IsExclusiveEventActive() => _activeEventInfos.Values.Any(info => info.IsExclusive);

        private async UniTaskVoid RunEventAsync(
            RandomEventDefinition definition,
            RandomEventInfo info,
            RandomEventActionPlan plan,
            CancellationToken token)
        {
            var isGated = false;

            try
            {
                isGated = !await plan.ExecuteAsync(token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                Debug.LogError($"Random event '{definition.name}' failed with exception: {exception}");
            }
            finally
            {
                if (_activeTokenSources.TryGetValue(definition, out var tokenSource))
                {
                    _activeTokenSources.Remove(definition);
                    tokenSource.Dispose();
                }

                _activeEventInfos.Remove(definition);
                _activePlans.Remove(definition);

                if (!isGated)
                {
                    _cooldownEndTimes[definition] = Time.time + definition.CooldownSeconds;
                    _lastEventEndTime = Time.time;
                }

                EventFinished?.Invoke(info);
            }
        }
    }
}
