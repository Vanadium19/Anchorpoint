using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sirenix.OdinInspector;
using UnityEngine;

namespace AudioModule
{
    public sealed partial class AudioSystem
    {
        private static readonly AnimationCurve s_FadeOutLinearCurve = AnimationCurve.Linear(0, 1, 1, 0);

        [FoldoutGroup("Events")]
        [ShowInInspector, ReadOnly, HideInEditorMode]
        private readonly Dictionary<AudioEventBase, string> _createdEvents = new();
        private readonly List<KeyValuePair<AudioEventBase, string>> _createdEventsCache = new();

        [FoldoutGroup("Events")]
        [ShowInInspector, ReadOnly, HideInEditorMode]
        private readonly HashSet<AudioEventBase> _playingEvents = new();
        private readonly List<AudioEventBase> _playingEventsCache = new();

        [FoldoutGroup("Events")]
        [ShowInInspector, ReadOnly, HideInEditorMode]
        private readonly Dictionary<string, float> _maxFrequencyEvents = new();
        private readonly List<KeyValuePair<string, float>> _maxFrequencyEventsCache = new();

        [FoldoutGroup("Events")]
        [ShowInInspector, ReadOnly, HideInEditorMode]
        private readonly Dictionary<AudioEventBase, Transform> _eventPivots = new();

        [ShowInInspector, ReadOnly, HideInInspector]
        private readonly Dictionary<AudioEventBase, Coroutine> _fadeouts = new();

        #region Play

        [Title("Methods")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PlayEvent(string eventId, float maxFrequency = 0) =>
            this.PlayEvent(eventId, Vector3.zero, Quaternion.identity, maxFrequency);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PlayEvent(string eventId, out AudioEventHandle handle, float maxFrequency = 0) =>
            this.PlayEvent(eventId, Vector3.zero, Quaternion.identity, out handle, maxFrequency);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PlayEvent(string eventId, Vector3 position, float maxFrequency = 0) =>
            this.PlayEvent(eventId, position, Quaternion.identity, maxFrequency);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PlayEvent(string eventId, Vector3 position, out AudioEventHandle handle, float maxFrequency = 0) =>
            this.PlayEvent(eventId, position, Quaternion.identity, out handle, maxFrequency);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PlayEvent(string eventId, Vector3 position, Quaternion rotation, float maxFrequency = 0)
            => this.PlayEvent(eventId, position, rotation, out _, maxFrequency);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PlayEvent(string eventId, Transform pivot, float maxFrequency = 0) =>
            this.PlayEvent(eventId, pivot, out _, maxFrequency);

        [Button, GUIColor(1f, 0.83f, 0f), HideInEditorMode]
        public bool PlayEvent(
            string eventId,
            Vector3 position,
            Quaternion rotation,
            out AudioEventHandle handle,
            float maxFrequency = 0
        )
        {
            if (_maxFrequencyEvents.ContainsKey(eventId))
            {
                handle = default;
                return false;
            }

            if (!this.audioEventPool.TryGet(eventId, out AudioEventBase audioEvent))
            {
                Debug.LogWarning("Audio event with id " + eventId + " not found");
                handle = default;
                return false;
            }

            audioEvent.Position = position;
            audioEvent.Rotation = rotation;
            audioEvent.SetCallback(this.DisposeEvent);

            _createdEvents.Add(audioEvent, eventId);

            if (maxFrequency > 0)
                _maxFrequencyEvents.Add(eventId, maxFrequency);

            this.StartEvent(audioEvent);
            handle = new AudioEventHandle(audioEvent, this);
            return true;
        }

        [Button, GUIColor(1f, 0.83f, 0f), HideInEditorMode]
        public bool PlayEvent(string eventId, Transform pivot, out AudioEventHandle handle, float maxFrequency = 0)
        {
            if (_maxFrequencyEvents.ContainsKey(eventId))
            {
                handle = default;
                return false;
            }

            if (!this.audioEventPool.TryGet(eventId, out AudioEventBase audioEvent))
            {
                handle = default;
                return false;
            }

            audioEvent.Position = pivot.position;
            audioEvent.Rotation = pivot.rotation;
            audioEvent.SetCallback(this.DisposeEvent);

            _createdEvents.Add(audioEvent, eventId);
            _eventPivots.Add(audioEvent, pivot);

            if (maxFrequency > 0)
                _maxFrequencyEvents.Add(eventId, maxFrequency);

            this.StartEvent(audioEvent);
            handle = new AudioEventHandle(audioEvent, this);
            return true;
        }

        #endregion

        #region Create

        public AudioEventHandle CreateEvent(string eventId) =>
            this.CreateEvent(eventId, Vector3.zero, Quaternion.identity);

        public AudioEventHandle CreateEvent(string eventId, Vector3 position, Quaternion rotation)
        {
            AudioEventBase audioEvent = this.audioEventPool.Get(eventId);
            audioEvent.Position = position;
            audioEvent.Rotation = rotation;
            audioEvent.SetCallback(null);

            _createdEvents.Add(audioEvent, eventId);

            return new AudioEventHandle(audioEvent, this);
        }

        public bool TryCreateEvent(string eventId, out AudioEventHandle result) =>
            this.TryCreateEvent(eventId, Vector3.zero, Quaternion.identity, out result);

        public bool TryCreateEvent(string eventId, Vector3 position, Quaternion rotation, out AudioEventHandle result)
        {
            if (!this.audioEventPool.TryGet(eventId, out AudioEventBase audioEvent))
            {
                result = default;
                return false;
            }

            audioEvent.Position = position;
            audioEvent.Rotation = rotation;
            audioEvent.SetCallback(null);

            _createdEvents.Add(audioEvent, eventId);

            result = new AudioEventHandle(audioEvent, this);
            return true;
        }

        internal bool IsCreatedEvent(AudioEventBase audioEvent)
        {
            return _createdEvents.ContainsKey(audioEvent);
        }

        #endregion

        #region Dispose

        public bool DisposeEvent(string eventId)
        {
            _createdEventsCache.Clear();
            _createdEventsCache.AddRange(_createdEvents);

            for (int i = 0, count = _createdEventsCache.Count; i < count; i++)
            {
                (AudioEventBase audioEvent, string fullId) = _createdEventsCache[i];
                if (fullId == eventId)
                {
                    this.DisposeEvent(audioEvent);
                    return true;
                }
            }

            return false;
        }

        public void DisposeEvents(string eventId)
        {
            _createdEventsCache.Clear();
            _createdEventsCache.AddRange(_createdEvents);

            for (int i = 0, count = _createdEventsCache.Count; i < count; i++)
            {
                (AudioEventBase audioEvent, string fullId) = _createdEventsCache[i];
                if (fullId == eventId)
                    this.DisposeEvent(audioEvent);
            }
        }

        internal void DisposeEvent(AudioEventBase audioEvent)
        {
            if (!_createdEvents.Remove(audioEvent))
                return;

            if (_playingEvents.Remove(audioEvent))
                audioEvent.OnStop();

            _eventPivots.Remove(audioEvent);
            this.audioEventPool.Release(audioEvent);
        }

        public bool DisposeEvent(string eventId, float fadeoutTime) =>
            this.DisposeEvent(eventId, s_FadeOutLinearCurve, fadeoutTime);

        public void DisposeEvents(string eventId, float fadeoutTime) =>
            this.DisposeEvents(eventId, s_FadeOutLinearCurve, fadeoutTime);

        public bool DisposeEvent(string eventId, AnimationCurve curve, float fadeoutTime)
        {
            _createdEventsCache.Clear();
            _createdEventsCache.AddRange(_createdEvents);

            for (int i = 0, count = _createdEventsCache.Count; i < count; i++)
            {
                (AudioEventBase audioEvent, string fullId) = _createdEventsCache[i];
                if (fullId == eventId)
                {
                    this.StartCoroutine(this.DisposeEventRoutine(audioEvent, curve, fadeoutTime));
                    return true;
                }
            }

            return false;
        }

        private IEnumerator DisposeEventRoutine(AudioEventBase audioEvent, AnimationCurve curve, float fadeoutTime)
        {
            if (!_createdEvents.Remove(audioEvent))
                yield break;

            if (_playingEvents.Remove(audioEvent))
            {
                yield return audioEvent.FadeOut(curve, fadeoutTime);
                audioEvent.OnStop();
            }

            _eventPivots.Remove(audioEvent);
            this.audioEventPool.Release(audioEvent);
        }

        public void DisposeEvents(string eventId, AnimationCurve curve, float fadeoutTime)
        {
            _createdEventsCache.Clear();
            _createdEventsCache.AddRange(_createdEvents);

            for (int i = 0, count = _createdEventsCache.Count; i < count; i++)
            {
                (AudioEventBase audioEvent, string fullId) = _createdEventsCache[i];
                if (fullId == eventId)
                    this.StartCoroutine(this.DisposeEventRoutine(audioEvent, curve, fadeoutTime));
            }
        }

        #endregion

        #region Find

        public bool FindEvent(string eventId, out AudioEventHandle result)
        {
            foreach ((AudioEventBase audioEventBase, string fullKey) in _createdEvents)
            {
                if (fullKey == eventId)
                {
                    result = new AudioEventHandle(audioEventBase, this);
                    return true;
                }
            }

            result = default;
            return false;
        }

        public int FindEventsNonAlloc(string eventId, AudioEventHandle[] results)
        {
            int count = 0;

            foreach ((AudioEventBase audioEventBase, string fullKey) in _createdEvents)
                if (fullKey == eventId)
                    results[count++] = new AudioEventHandle(audioEventBase, this);

            return count;
        }

        #endregion

        #region Lifecycle

        internal void StartEvent(AudioEventBase audioEvent)
        {
            if (_fadeouts.Remove(audioEvent, out Coroutine coroutine)) 
                this.StopCoroutine(coroutine);

            if (_playingEvents.Add(audioEvent))
                audioEvent.OnStart();
        }

        internal bool IsPlayingEvent(AudioEventBase audioEvent)
        {
            return _playingEvents.Contains(audioEvent);
        }

        internal void StopEvent(AudioEventBase audioEvent, float fadeoutTime)
        {
            this.StopEvent(audioEvent, fadeoutTime, s_FadeOutLinearCurve);
        }

        internal void StopEvent(AudioEventBase audioEvent, float fadeoutTime, AnimationCurve curve)
        {
            if (_fadeouts.Remove(audioEvent, out Coroutine coroutine)) 
                this.StopCoroutine(coroutine);

            coroutine = this.StartCoroutine(this.StopEventRoutine(audioEvent, fadeoutTime, curve));
            _fadeouts.Add(audioEvent, coroutine);
        }

        private IEnumerator StopEventRoutine(AudioEventBase audioEvent, float fadeoutTime, AnimationCurve curve)
        {
            yield return audioEvent.FadeOut(curve, fadeoutTime);
            if (_playingEvents.Remove(audioEvent)) 
                audioEvent.OnStop();

            _fadeouts.Remove(audioEvent);
        }

        internal void StopEvent(AudioEventBase audioEvent)
        {
            if (_playingEvents.Remove(audioEvent))
                audioEvent.OnStop();
        }

        #endregion

        private void UpdatePlayingEvents(float deltaTime)
        {
            _playingEventsCache.Clear();
            _playingEventsCache.AddRange(_playingEvents);

            for (int i = 0, count = _playingEventsCache.Count; i < count; i++)
            {
                AudioEventBase audioEvent = _playingEventsCache[i];
                audioEvent.OnUpdate(deltaTime);
            }
        }

        private void UpdateMaxFrequencyEvents(float deltaTime)
        {
            _maxFrequencyEventsCache.Clear();
            _maxFrequencyEventsCache.AddRange(_maxFrequencyEvents);

            for (int i = 0, count = _maxFrequencyEventsCache.Count; i < count; i++)
            {
                (string eventId, float maxFrequency) = _maxFrequencyEventsCache[i];
                maxFrequency -= deltaTime;

                if (maxFrequency <= 0)
                    _maxFrequencyEvents.Remove(eventId);
                else
                    _maxFrequencyEvents[eventId] = maxFrequency;
            }
        }

        private void UpdatePivotEvents()
        {
            foreach ((AudioEventBase audioEvent, Transform pivot) in _eventPivots)
            {
                audioEvent.Position = pivot.position;
                audioEvent.Rotation = pivot.rotation;
            }
        }
    }
}