using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AudioModule;
using BuildingModule;
using Cysharp.Threading.Tasks;
using InputModule;
using PlayerModule;
using UnityEngine;
using UtilsModule;
using VFXModule;
using Object = UnityEngine.Object;

namespace RandomEventsModule
{
    /// <summary>Ignites a random building, spreads the fire to nearby buildings over time, and lets the player extinguish one by holding the interact button next to it.</summary>
    public class FireEventAction : RandomEventActionBase
    {
        private readonly IBuildingRegistry _registry;
        private readonly IEffectsService _effects;
        private readonly IAudioSystem _audioSystem;
        private readonly PlayerProvider _player;
        private readonly IInputMap _input;
        private readonly RandomEventProgressHudView _hudView;
        private readonly string _ignitionAudioEventId;
        private readonly string _burningAudioEventId;
        private readonly string _extinguishAudioEventId;
        private readonly string _extinguishHintKey;
        private readonly float _spreadRadius;
        private readonly float _spreadIntervalSeconds;
        private readonly int _maxBurningBuildings;
        private readonly float _extinguishRadius;
        private readonly float _extinguishSeconds;
        private readonly Dictionary<BuildingView, BurningState> _burning = new();

        private bool _stopRequested;

        /// <summary>Creates the action with its spread/extinguish tuning.</summary>
        public FireEventAction(
            IBuildingRegistry registry,
            IEffectsService effects,
            IAudioSystem audioSystem,
            PlayerProvider player,
            IInputMap input,
            RandomEventProgressHudView hudView,
            string ignitionAudioEventId,
            string burningAudioEventId,
            string extinguishAudioEventId,
            string extinguishHintKey,
            float spreadRadius,
            float spreadIntervalSeconds,
            int maxBurningBuildings,
            float extinguishRadius,
            float extinguishSeconds)
        {
            _registry = registry;
            _effects = effects;
            _audioSystem = audioSystem;
            _player = player;
            _input = input;
            _hudView = hudView;
            _ignitionAudioEventId = ignitionAudioEventId;
            _burningAudioEventId = burningAudioEventId;
            _extinguishAudioEventId = extinguishAudioEventId;
            _extinguishHintKey = extinguishHintKey;
            _spreadRadius = spreadRadius;
            _spreadIntervalSeconds = spreadIntervalSeconds;
            _maxBurningBuildings = maxBurningBuildings;
            _extinguishRadius = extinguishRadius;
            _extinguishSeconds = extinguishSeconds;
        }

        /// <inheritdoc/>
        public override async UniTask<bool> ExecuteAsync(CancellationToken token)
        {
            var first = PickTarget();

            if (first == null)
                return false;

            Ignite(first);

            var spreadTimer = 0f;

            while (_burning.Count > 0 && !_stopRequested)
            {
                var canceled = await UniTask.Yield(PlayerLoopTiming.Update, token).SuppressCancellationThrow();

                if (canceled)
                    break;

                var deltaTime = Time.deltaTime;
                spreadTimer += deltaTime;

                if (spreadTimer >= _spreadIntervalSeconds)
                {
                    spreadTimer = 0f;
                    TrySpread();
                }

                Tick(deltaTime);
            }

            StopAllBurning();
            _burning.Clear();

            if (_hudView != null)
                _hudView.Hide();

            return true;
        }

        /// <inheritdoc/>
        public override void RequestStop() => _stopRequested = true;

        private BuildingView PickTarget() =>
            _registry.Buildings
                .Where(building => building != null && !_burning.ContainsKey(building))
                .OrderBy(_ => Random.value)
                .FirstOrDefault();

        private void TrySpread()
        {
            if (_burning.Count >= _maxBurningBuildings)
                return;

            foreach (var source in _burning.Keys.ToList())
            {
                var neighbor = _registry.Buildings.FirstOrDefault(building =>
                    building != null &&
                    !_burning.ContainsKey(building) &&
                    Vector3.Distance(building.transform.position, source.transform.position) <= _spreadRadius);

                if (neighbor == null)
                    continue;

                Ignite(neighbor);

                return;
            }
        }

        private void Ignite(BuildingView building)
        {
            var state = new BurningState();
            var worldBounds = GetWorldBounds(building);
            var position = worldBounds.center;
            var scale = worldBounds.size;
            scale.y = 1.0f;
            state.Bounds = worldBounds;

            if (_audioSystem.PlayEvent(_burningAudioEventId, position, out var burningSound))
                state.BurningSound = burningSound;

            _burning[building] = state;
            state.Effect = _effects.Fire(EffectId.Fire, building.transform.position, Quaternion.identity, building.transform, scale);
            _audioSystem.PlayEvent(_ignitionAudioEventId, position);
        }

        private static Bounds GetWorldBounds(BuildingView building)
        {
            var renderers = building.GetAllMeshRenderers();

            if (renderers.Length == 0)
                return new Bounds(building.transform.position, Vector3.one);

            var bounds = renderers[0].bounds;

            for (var i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            return bounds;
        }

        private void Tick(float deltaTime)
        {
            var extinguishSquaredRadius = _extinguishRadius * _extinguishRadius;

            BuildingView nearestInRange = null;
            BurningState nearestState = null;
            var nearestSquaredDistance = float.MaxValue;

            foreach (var pair in _burning.ToList())
            {
                var building = pair.Key;
                var state = pair.Value;

                if (building == null)
                {
                    StopBurning(state);
                    _burning.Remove(building);
                    continue;
                }

                var squaredDistance = state.Bounds.SqrDistance(_player.Position);

                if (squaredDistance <= extinguishSquaredRadius && squaredDistance < nearestSquaredDistance)
                {
                    nearestSquaredDistance = squaredDistance;
                    nearestInRange = building;
                    nearestState = state;
                }
            }

            var isExtinguishing = nearestState != null && _input.IsInteractHeld;

            // Only the closest fire in reach is worked on; every other fire loses its progress.
            foreach (var state in _burning.Values)
                if (state != nearestState || !isExtinguishing)
                    state.ExtinguishProgress = 0f;

            if (isExtinguishing)
            {
                nearestState.ExtinguishProgress += deltaTime;

                if (nearestState.ExtinguishProgress >= _extinguishSeconds)
                {
                    Extinguish(nearestInRange, nearestState);
                    _burning.Remove(nearestInRange);
                    nearestInRange = null;
                    nearestState = null;
                }
            }

            UpdateHud(nearestInRange, nearestState);
        }

        private void Extinguish(BuildingView building, BurningState state)
        {
            StopBurning(state);
            _audioSystem.PlayEvent(_extinguishAudioEventId, building.transform.position);
        }

        private void StopAllBurning()
        {
            foreach (var state in _burning.Values)
                StopBurning(state);
        }

        private static void StopBurning(BurningState state)
        {
            if (state.Effect is Object effectObject && effectObject != null)
                state.Effect.Stop();

            state.BurningSound.Stop();
            state.BurningSound.Dispose();
        }

        private void UpdateHud(BuildingView nearestInRange, BurningState state)
        {
            if (nearestInRange == null)
            {
                _hudView.Hide();
                return;
            }

            _hudView.Show(LocalizedText.Get(_extinguishHintKey));
            _hudView.SetProgress(_extinguishSeconds > 0f ? state.ExtinguishProgress / _extinguishSeconds : 1f);
        }

        private class BurningState
        {
            public IEffectHandle Effect;
            public AudioEventHandle BurningSound;
            public Bounds Bounds;
            public float ExtinguishProgress;
        }
    }
}
