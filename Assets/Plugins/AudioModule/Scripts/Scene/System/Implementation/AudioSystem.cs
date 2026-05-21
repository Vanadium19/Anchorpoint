using Sirenix.OdinInspector;
using UnityEngine;

// ReSharper disable ArrangeObjectCreationWhenTypeNotEvident

namespace AudioModule
{
    [DefaultExecutionOrder(-1000)]
    [AddComponentMenu("Audio/Audio System", -100)]
    public sealed partial class AudioSystem : MonoBehaviour, IAudioSystem
    {
        [SerializeField]
        private bool initOnAwake = true;

        [FoldoutGroup("Pools")]
        [ShowInInspector, ReadOnly, HideInEditorMode]
        private AudioSourcePool audioSourcePool;

        [FoldoutGroup("Pools")]
        [ShowInInspector, ReadOnly, HideInEditorMode]
        private AudioEventPool audioEventPool;

        public void Initialize()
        {
            this.audioSourcePool = new AudioSourcePool(this.transform);
            this.audioEventPool = new AudioEventPool(this);
            this.LoadInitialBanks();
        }

        [Button, GUIColor(1f, 0.83f, 0f), HideInEditorMode]
        public void Pause()
        {
            this.enabled = false;

            foreach (AudioEventBase playingEvent in _playingEvents) 
                playingEvent.OnPause();
        }

        [Button, GUIColor(1f, 0.83f, 0f), HideInEditorMode]
        public void Resume()
        {
            this.enabled = true;

            foreach (AudioEventBase playingEvent in _playingEvents) 
                playingEvent.OnResume();
        }

        internal AudioSource TakeAudioSource()
        {
            return this.audioSourcePool.Take();
        }

        internal void ReleaseAudioSource(AudioSource audioSource)
        {
            this.audioSourcePool.Release(audioSource);
        }

        private void Awake()
        {
            if (this.initOnAwake) this.Initialize();
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            this.UpdatePivotEvents();
            this.UpdatePlayingEvents(deltaTime);
            this.UpdateMaxFrequencyEvents(deltaTime);
        }

        private void OnDestroy()
        {
            this.audioSourcePool.Dispose();
            this.audioEventPool.Dispose();
        }
    }
}