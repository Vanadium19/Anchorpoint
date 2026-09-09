using System.Threading;
using AudioModule;
using ComponentsModule;
using Cysharp.Threading.Tasks;
using PlayerModule;
using UnityEngine;
using VFXModule;
using Object = UnityEngine.Object;

namespace RandomEventsModule
{
    /// <summary>Emits radiation on the player at the base, dealing customizable damage over time with spatial effects.</summary>
    /// <remarks>
    /// Player health is accessed lazily via <see cref="PlayerProvider.TryGet{T}"/> to the sub-container's <see cref="IHealthComponent"/>.
    /// The VFX is parented to the main camera so it fills the player's view; the looping Geiger-counter sound follows the player. Both are spawned once, held for the duration, then stopped through their handles.
    /// </remarks>
    public class RadiationSurgeAction : RandomEventActionBase
    {
        private readonly PlayerProvider _player;
        private readonly IRandomEventClock _clock;
        private readonly IEffectsService _effects;
        private readonly IAudioSystem _audioSystem;
        private readonly float _durationSeconds;
        private readonly float _damagePerTick;
        private readonly float _tickIntervalSeconds;
        private readonly string _geigerAudioEventId;

        private bool _stopRequested;

        /// <summary>Creates the action with its damage and effect timing.</summary>
        public RadiationSurgeAction(
            PlayerProvider player,
            IRandomEventClock clock,
            IEffectsService effects,
            IAudioSystem audioSystem,
            float durationSeconds,
            float damagePerTick,
            float tickIntervalSeconds,
            string geigerAudioEventId)
        {
            _player = player;
            _clock = clock;
            _effects = effects;
            _audioSystem = audioSystem;
            _durationSeconds = durationSeconds;
            _damagePerTick = damagePerTick;
            _tickIntervalSeconds = tickIntervalSeconds;
            _geigerAudioEventId = geigerAudioEventId;
        }

        /// <inheritdoc/>
        public override async UniTask<bool> ExecuteAsync(CancellationToken token)
        {
            var elapsedTime = 0f;
            var damageTimer = 0f;

            var camera = Camera.main;
            var effectPivot = camera != null ? camera.transform : _player.transform;
            var effect = _effects.Fire(EffectId.Radiation, effectPivot.position, Quaternion.identity, effectPivot);
            var hasGeiger = _audioSystem.PlayEvent(_geigerAudioEventId, _player.transform, out var geiger);

            while (elapsedTime < _durationSeconds && !_stopRequested)
            {
                var canceled = await UniTask.Yield(PlayerLoopTiming.Update, token).SuppressCancellationThrow();

                if (canceled)
                    break;

                if (_clock.IsPaused)
                    continue;

                var deltaTime = Time.deltaTime;
                elapsedTime += deltaTime;
                damageTimer += deltaTime;

                if (damageTimer >= _tickIntervalSeconds)
                {
                    damageTimer = 0f;
                    TakeDamage();
                }
            }

            if (effect is Object effectObject && effectObject != null)
                effect.Stop();

            if (hasGeiger)
            {
                geiger.Stop();
                geiger.Dispose();
            }

            return true;
        }

        /// <inheritdoc/>
        public override void RequestStop() => _stopRequested = true;

        private void TakeDamage()
        {
            if (_player.TryGet<IHealthComponent>(out var health) && health.IsAlive)
                health.TakeDamage(_damagePerTick);
        }
    }
}
