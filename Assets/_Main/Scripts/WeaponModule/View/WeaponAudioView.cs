using System;
using System.Collections.Generic;
using AudioModule;
using UnityEngine;
using Zenject;

namespace WeaponModule
{
    /// <summary>
    /// All sounds of a weapon. Every sound is bound to a weapon event id (<see cref="WeaponEventIds"/>
    /// or an id raised by a clip), so state moments and clip moments are handled the same way.
    /// </summary>
    public class WeaponAudioView : WeaponViewPart
    {
        [SerializeField] private SoundBinding[] sounds = Array.Empty<SoundBinding>();
        [SerializeField] private float fireAudioMinInterval = 0.05f;
        [SerializeField, Min(1)] private int fireAudioMaxVoices = 2;
        [SerializeField] private float fireAudioStealFadeOut = 0.06f;
        [SerializeField] private float mechanismAudioFadeOut = 0.1f;

        private readonly List<AudioEventHandle> _fireHandles = new();
        private readonly List<AudioEventHandle> _stoppingHandles = new();

        private IAudioSystem _audioSystem;
        private AudioEventHandle _mechanismHandle;

        private void OnDestroy()
        {
            foreach (var handle in _fireHandles)
                handle.Dispose();

            foreach (var handle in _stoppingHandles)
                handle.Dispose();

            _mechanismHandle.Dispose();
        }

        [Inject]
        private void Construct(IAudioSystem audioSystem) => _audioSystem = audioSystem;

        public override void OnFired() => PlayFireSound();

        public override void OnReloadStarted(bool isEmptyReload)
        {
            StopMechanismSound();

            var eventId = isEmptyReload && TryGetSound(WeaponEventIds.EmptyReloadStarted, out _)
                ? WeaponEventIds.EmptyReloadStarted
                : WeaponEventIds.ReloadStarted;

            PlayMechanismSound(eventId);
        }

        public override void OnReloadStateChanged(bool isReloading)
        {
            if (!isReloading)
                StopMechanismSound();
        }

        public override void OnRoundInserted()
        {
            if (_audioSystem != null && TryGetSound(WeaponEventIds.RoundInserted, out var sound))
                _audioSystem.PlayEvent(sound, transform);
        }

        public override void OnAnimationEvent(string eventId) => PlayMechanismSound(eventId);

        public override void ResetVisuals()
        {
            StopMechanismSound();
            ReleaseStoppedVoices();
        }

        private bool TryGetSound(string eventId, out string sound)
        {
            foreach (var binding in sounds)
            {
                if (binding.EventId != eventId || string.IsNullOrEmpty(binding.Sound))
                    continue;

                sound = binding.Sound;
                return true;
            }

            sound = null;
            return false;
        }

        private void PlayFireSound()
        {
            if (_audioSystem == null || !TryGetSound(WeaponEventIds.Fired, out var sound))
                return;

            ReleaseStoppedVoices();
            _fireHandles.RemoveAll(handle => !handle.IsValid);

            while (_fireHandles.Count >= fireAudioMaxVoices)
            {
                _fireHandles[0].Stop(fireAudioStealFadeOut);
                _stoppingHandles.Add(_fireHandles[0]);
                _fireHandles.RemoveAt(0);
            }

            if (_audioSystem.PlayEvent(sound, transform, out var fireHandle, fireAudioMinInterval))
                _fireHandles.Add(fireHandle);
        }

        private void ReleaseStoppedVoices()
        {
            for (var i = _stoppingHandles.Count - 1; i >= 0; i--)
            {
                var handle = _stoppingHandles[i];

                if (handle.IsValid && handle.IsPlaying)
                    continue;

                handle.Dispose();
                _stoppingHandles.RemoveAt(i);
            }
        }

        private void PlayMechanismSound(string eventId)
        {
            if (_audioSystem != null && TryGetSound(eventId, out var sound))
                _audioSystem.PlayEvent(sound, transform, out _mechanismHandle);
        }

        private void StopMechanismSound()
        {
            if (_mechanismHandle.IsValid)
            {
                _mechanismHandle.Stop(mechanismAudioFadeOut);
                _stoppingHandles.Add(_mechanismHandle);
            }

            _mechanismHandle = default;
        }

        [Serializable]
        private struct SoundBinding
        {
            [SerializeField] private string eventId;
            [SerializeField] private AudioEventKey sound;

            public string EventId => eventId;
            public string Sound => sound;
        }
    }
}
