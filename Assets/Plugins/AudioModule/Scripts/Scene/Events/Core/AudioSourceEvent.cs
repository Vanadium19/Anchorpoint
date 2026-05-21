using System.Collections;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable UnassignedField.Local

namespace AudioModule
{
    [CreateAssetMenu(fileName = "AudioEvent", menuName = "Game/Audio/Scene/AudioEvent")]
    public class AudioSourceEvent : AudioEventBase
    {
        public AudioSystem System => _audioSystem;

        public AudioSource Source => _audioSource;

        public float CurrentTime => _currentTime;

        public float CurrentProgress => _currentProgress;

        [Title("Main")]
        [SerializeField]
        private float duration;

        [SerializeField]
        private bool loop;

        [SerializeField, ShowIf(nameof(loop))]
        private bool loopOne;

        [SuffixLabel("2D — 3D")]
        [SerializeField, Range(0, 1)]
        private float spartialBlend;

        [SerializeField]
        private AudioMixerGroup output;

        [Title("Audio")]
        [SerializeReference]
        private IClipProvider clip = new ClipConst();

        [SerializeReference, Space]
        private IFloatProvider pitch = new FloatConst();

        [SerializeReference, Space]
        private IFloatProvider volume = new FloatConst();

        [SerializeReference, Space]
        private IFloatProvider reverbZoneMix = new FloatConst(1);

        [FoldoutGroup("3D")]
        [SerializeField, Space]
        private AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;

        [FoldoutGroup("3D")]
        [SerializeField, ShowIf(nameof(rolloffMode), AudioRolloffMode.Custom)]
        private AnimationCurve rolloffCurve;

        [FoldoutGroup("3D")]
        [SerializeReference, Space]
        private IFloatProvider minDistance = new FloatConst(1);

        [FoldoutGroup("3D")]
        [SerializeReference]
        private IFloatProvider maxDistance = new FloatConst(500);

        [FoldoutGroup("3D")]
        [SerializeReference]
        private IFloatProvider dopplerLevel = new FloatConst(0.8f);

        [FoldoutGroup("3D")]
        [SerializeReference]
        private IFloatProvider spread = new FloatConst(0);

        [SerializeField]
        private BehaviourInfo[] behaviours;

        [SerializeField, Space]
        private ActionInfo[] actions;

        private AudioSource _audioSource;
        private Transform _audioSourceTransform;

        private bool _isFirstCycle;
        private float _currentTime;
        private float _currentProgress;
        private float _currentDuration;

        internal protected override Vector3 Position
        {
            get => _position;
            set {
                _position = value;
                if (_audioSourceTransform != null) _audioSourceTransform.position = value;
            }
        }

        internal protected override Quaternion Rotation
        {
            get => _rotation;
            set {
                this._rotation = value;
                if (_audioSourceTransform != null) _audioSourceTransform.rotation = value;
            }
        }

        internal protected override void OnStart()
        {
            _audioSource = _audioSystem.TakeAudioSource();

            _audioSource.name = this._identifier;
            _audioSource.outputAudioMixerGroup = this.output;
            _audioSource.loop = this.loop;
            _audioSource.spatialBlend = this.spartialBlend;

            _audioSourceTransform = _audioSource.transform;
            _audioSourceTransform.position = _position;
            _audioSourceTransform.rotation = _rotation;

            _isFirstCycle = true;

            this.StartBehaviours();
            this.ResetState();

            _audioSource.Play();
        }

        internal protected override void OnStop()
        {
            this.StopBehaviours();
            _audioSystem.ReleaseAudioSource(_audioSource);
            _audioSource = null;
        }

        internal protected override void OnUpdate(float deltaTime)
        {
            this.StartUpdate(deltaTime);
            this.ProcessUpdate();
            this.FinishUpdate();
        }

        private void FinishUpdate()
        {
            if (_currentProgress < 1)
                return;

            if (this.loop)
                ResetOnLoop();
            else
                this.Complete();
        }

        private void ResetOnLoop()
        {
            if (_isFirstCycle)
                this.StopNonLoopBehaviours();

            _isFirstCycle = false;
            this.ResetState(!this.loopOne);
            _audioSource.Play();
        }

        internal protected override void OnPause()
        {
            if (_audioSource.isPlaying)
                _audioSource.Pause();
        }

        internal protected override void OnResume()
        {
            if (!_audioSource.isPlaying)
                _audioSource.UnPause();
        }

        internal protected override IEnumerator FadeOut(AnimationCurve curve, float duration)
        {
            float time = 0f;
            float startVolume = _audioSource.volume;

            while (time < duration)
            {
                float t = time / duration;
                float fadeMultiplier = curve?.Evaluate(t) ?? 1f - t;
                _audioSource.volume = startVolume * fadeMultiplier;

                time += Time.deltaTime;
                yield return null;
            }

            _audioSource.volume = 0f;
        }

        private void ResetState(bool resetSettings = true)
        {
            if (resetSettings)
                this.ResetSettings();

            this.ResetPlaybackState();
            this.ResetAdditionalState();
        }

        private void ResetSettings()
        {
            _audioSource.volume = this.volume?.Value ?? 1;
            _audioSource.pitch = this.pitch?.Value ?? 1;
            _audioSource.clip = this.clip?.Value;
            _audioSource.reverbZoneMix = reverbZoneMix?.Value ?? 1;

            _currentDuration = Mathf.Min(duration, clip?.MaxLength ?? 0);

            _audioSource.dopplerLevel = this.dopplerLevel.Value;
            _audioSource.spread = this.spread.Value;
            _audioSource.minDistance = this.minDistance.Value;
            _audioSource.maxDistance = this.maxDistance.Value;
            _audioSource.rolloffMode = this.rolloffMode;

            if (this.rolloffMode == AudioRolloffMode.Custom)
                _audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, this.rolloffCurve);
        }

        private void ResetPlaybackState()
        {
            _currentTime = 0;
            _currentProgress = 0;
        }

        private void ResetAdditionalState()
        {
            this.ResetActions();
            this.ResetBehaviours();
        }

        private void ResetBehaviours()
        {
            for(int i = 0, count = this.behaviours.Length; i < count; i++)
            {
                BehaviourInfo behaviour = this.behaviours[i];

                if (behaviour.mute)
                    continue;

                if (!this.IsBehaviourActiveInCurrentCycle(behaviour))
                    continue;

                behaviour.value.OnReset(this);
            }
        }

        private void ResetActions()
        {
            for(int i = 0, count = this.actions.Length; i < count; i++)
            {
                ActionInfo action = this.actions[i];
                action.passed = false;
            }
        }

        private void StartUpdate(float deltaTime)
        {
            _currentTime = Mathf.Min(_currentTime + deltaTime, _currentDuration);
            _currentProgress = Mathf.Clamp01(_currentTime / _currentDuration);
        }

        private void ProcessUpdate()
        {
            ProcessActions();
            ProcessBehaviours();
        }

        private void ProcessActions()
        {
            for(int i = 0, count = this.actions.Length; i < count; i++)
            {
                ActionInfo action = this.actions[i];

                if (action.mute)
                    continue;

                if (!action.passed && _currentTime >= action.time)
                {
                    action.value.Invoke(this);
                    action.passed = true;
                }
            }
        }

        private void ProcessBehaviours()
        {
            for(int i = 0, count = this.behaviours.Length; i < count; i++)
            {
                BehaviourInfo behaviour = this.behaviours[i];

                if (behaviour.mute)
                    continue;

                if (!this.IsBehaviourActiveInCurrentCycle(behaviour))
                    continue;

                behaviour.value.OnUpdate(this);
            }
        }

        private void StartBehaviours()
        {
            for(int i = 0, count = this.behaviours.Length; i < count; i++)
            {
                BehaviourInfo behaviour = this.behaviours[i];

                if (behaviour.mute)
                    continue;

                behaviour.value.OnStart(this);
            }
        }

        private void StopBehaviours()
        {
            for(int i = 0, count = this.behaviours.Length; i < count; i++)
            {
                BehaviourInfo behaviour = this.behaviours[i];

                if (behaviour.mute)
                    continue;

                if (!this.IsBehaviourActiveInCurrentCycle(behaviour))
                    continue;

                behaviour.value.OnStop(this);
            }
        }

        private void StopNonLoopBehaviours()
        {
            for(int i = 0, count = this.behaviours.Length; i < count; i++)
            {
                BehaviourInfo behaviour = this.behaviours[i];

                if (behaviour.mute || behaviour.loop)
                    continue;

                behaviour.value.OnStop(this);
            }
        }

        private bool IsBehaviourActiveInCurrentCycle(BehaviourInfo behaviour)
        {
            return _isFirstCycle || behaviour.loop;
        }

#if UNITY_EDITOR
        [Title("Tools")]
        [Button("Assign Duration From Clip")]
        [GUIColor(0, 1, 0)]
        private void AssignDurationFromClip()
        {
            this.duration = this.clip?.MaxLength ?? 0;
            AssetDatabase.SaveAssets();
        }
#endif
    }
}