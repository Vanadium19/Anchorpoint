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
    /// The service has no knowledge of any concrete event, only of conditions and actions.
    /// </summary>
    public class RandomEventService : IRandomEventService, IDisposable
    {
        private readonly RandomEventsRules _rules;
        private readonly DiContainer _container;
        private readonly Dictionary<RandomEventDefinition, CancellationTokenSource> _activeTokenSources = new();
        private readonly Dictionary<RandomEventDefinition, RandomEventInfo> _activeEventInfos = new();
        private readonly Dictionary<RandomEventDefinition, IReadOnlyList<IReadOnlyList<IRandomEventAction>>> _activeActions = new();
        private readonly Dictionary<RandomEventDefinition, float> _cooldownEndTimes = new();
        private readonly Dictionary<RandomEventPool, IRandomEventPicker> _pickers = new();

        /// <inheritdoc/>
        public event Action<RandomEventInfo> EventStarted;

        /// <inheritdoc/>
        public event Action<RandomEventInfo> EventFinished;

        /// <inheritdoc/>
        public bool IsAnyEventActive => _activeEventInfos.Count > 0;

        /// <inheritdoc/>
        public IReadOnlyList<RandomEventInfo> ActiveEvents => _activeEventInfos.Values.ToList();

        private float _lastEventEndTime = float.NegativeInfinity;

        /// <summary>Creates the service bound to one scene's rules and DI container.</summary>
        public RandomEventService(RandomEventsRules rules, DiContainer container)
        {
            _rules = rules;
            _container = container;
        }

        /// <inheritdoc/>
        public bool IsEventActive(RandomEventDefinition definition) => definition != null && _activeEventInfos.ContainsKey(definition);

        /// <inheritdoc/>
        public RandomEventStartResult Start(RandomEventDefinition definition, CancellationToken token)
        {
            if (definition == null)
                return RandomEventStartResult.NotFound;

            return StartDefinition(definition, token);
        }

        /// <inheritdoc/>
        public RandomEventStartResult StartRandom(RandomEventScope scope, CancellationToken token)
        {
            if (scope == null)
                return RandomEventStartResult.NoCandidates;

            if (IsGloballyBlocked())
                return RandomEventStartResult.Blocked;

            var result = RandomEventStartResult.NoCandidates;

            foreach (var pool in scope.Pools)
            {
                if (pool == null)
                    continue;

                var candidates = pool.Definitions
                    .Where(d => d != null && d.Weight > 0f && CanStartDefinition(d))
                    .ToList();

                if (candidates.Count == 0)
                    continue;

                var definition = GetPicker(pool).Pick(candidates);

                if (definition == null)
                    continue;

                var started = StartDefinition(definition, token);

                if (started == RandomEventStartResult.Started)
                    result = RandomEventStartResult.Started;
                else if (result != RandomEventStartResult.Started)
                    result = started;
            }

            return result;
        }

        /// <inheritdoc/>
        public void Stop(RandomEventDefinition definition)
        {
            if (definition == null || !_activeTokenSources.TryGetValue(definition, out var tokenSource))
                return;

            if (_activeActions.TryGetValue(definition, out var groups))
                foreach (var action in groups.SelectMany(group => group))
                    action.RequestStop();

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

        private RandomEventStartResult StartDefinition(RandomEventDefinition definition, CancellationToken token)
        {
            var blockReason = GetBlockReason(definition);

            if (blockReason.HasValue)
                return blockReason.Value;

            if (IsGloballyBlocked())
                return RandomEventStartResult.Blocked;

            var actionGroups = definition.CreateActionGroups(_container);
            var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
            var info = definition.CreateInfo();

            _activeTokenSources[definition] = tokenSource;
            _activeEventInfos[definition] = info;
            _activeActions[definition] = actionGroups;

            EventStarted?.Invoke(info);

            RunEventAsync(definition, info, actionGroups, tokenSource.Token).Forget();

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

        private IRandomEventPicker GetPicker(RandomEventPool pool)
        {
            if (_pickers.TryGetValue(pool, out var picker))
                return picker;

            picker = pool.CreatePicker(_container);
            _pickers[pool] = picker;

            return picker;
        }

        private bool IsOnCooldown(RandomEventDefinition definition) =>
            _cooldownEndTimes.TryGetValue(definition, out var cooldownEndTime) && Time.time < cooldownEndTime;

        private bool IsGloballyBlocked()
        {
            if (IsExclusiveEventActive())
                return true;

            return Time.time - _lastEventEndTime < _rules.MinIntervalBetweenEventsSeconds;
        }

        private bool IsExclusiveEventActive() => _activeEventInfos.Values.Any(info => info.IsExclusive);

        private async UniTaskVoid RunEventAsync(
            RandomEventDefinition definition,
            RandomEventInfo info,
            IReadOnlyList<IReadOnlyList<IRandomEventAction>> actionGroups,
            CancellationToken token)
        {
            var gateFailed = false;

            try
            {
                foreach (var group in actionGroups)
                {
                    var results = await UniTask.WhenAll(group.Select(action => action.ExecuteAsync(token)));

                    if (results.Any(passed => !passed))
                    {
                        gateFailed = true;
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Debug.LogError($"Random event '{definition.name}' failed with exception: {ex}");
            }
            finally
            {
                if (_activeTokenSources.TryGetValue(definition, out var tokenSource))
                {
                    _activeTokenSources.Remove(definition);
                    tokenSource.Dispose();
                }

                _activeEventInfos.Remove(definition);
                _activeActions.Remove(definition);
                _cooldownEndTimes[definition] = Time.time + definition.CooldownSeconds;

                if (!gateFailed)
                    _lastEventEndTime = Time.time;

                EventFinished?.Invoke(info);
            }
        }
    }
}
