using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace AudioModule
{
    [DefaultExecutionOrder(-100)]
    [AddComponentMenu("Audio/Music Player")]
    public sealed class MusicPlayer : MonoBehaviour
    {
        public enum State
        {
            IDLE = 0,
            PLAYING = 1,
            PAUSED = 2,
            FINISHED = 3
        }

        public event Action<bool> OnMuteChanged;
        public event Action<float> OnVolumeChanged;
        public event Action<float> OnPitchChanged;

        public event Action<AudioClip> OnTrackStarted;
        public event Action<AudioClip> OnTrackPaused;
        public event Action<AudioClip> OnTrackResumed;
        public event Action<AudioClip> OnTrackStopped;
        public event Action<AudioClip> OnTrackFinsihed;

        [ShowInInspector, PropertySpace(8), PropertyOrder(-20)]
        public bool IsMute
        {
            get { return this.GetMute(); }
            set { this.SetMute(value); }
        }

        [ShowInInspector, PropertyOrder(-18), PropertyRange(0, 1), PropertySpace]
        public float Volume
        {
            get { return this.GetVolume(); }
            set { this.SetVolume(value); }
        }

        [ShowInInspector, PropertyOrder(-18), PropertyRange(-3, 3)]
        public float Pitch
        {
            get { return this.GetPitch(); }
            set { this.SetPitch(value); }
        }

        [ShowInInspector, PropertyOrder(-17), PropertyRange(0, nameof(GetCurrentTrackLength)), HideInEditorMode]
        public float CurrentTime
        {
            get { return this.GetCurrentTime(); }
            set { this.SetCurrentTime(value); }
        }

        [ShowInInspector, ReadOnly, PropertySpace(8), PropertyOrder(-8), HideInEditorMode]
        public State CurrentState
        {
            get { return this.state; }
        }

        [ShowInInspector, ReadOnly, PropertyOrder(-7), HideInEditorMode]
        public AudioClip CurrentTrack
        {
            get { return this.GetCurrentTrack(); }
        }

        [ShowInInspector, ReadOnly, PropertyOrder(-6), HideInEditorMode]
        [ProgressBar(min: 0, max: 1, r: 1f, g: 0.83f, b: 0f)]
        public float CurrentProgress
        {
            get { return this.GetCurrentProgress(); }
            set { this.SetCurrentProgress(value); }
        }

        [SerializeField]
        private AudioSource audioSource;

        private State state;

        private void Awake()
        {
            this.state = State.IDLE;
        }

        private void Update()
        {
            if (this.state == State.PLAYING)
            {
                this.CheckTrackFinished();
            }
        }

        [Title("Methods")]
        [Button, GUIColor(1f, 0.83f, 0f), HideInEditorMode]
        public bool Play(AudioClip track)
        {
            if (track == null)
            {
                Debug.LogWarning("Track is null!");
                return false;
            }

            if (this.state is State.PLAYING or State.PAUSED)
            {
                Debug.LogWarning("Other music is already started!");
                return false;
            }
            
            if (this.audioSource == null)
            {
                Debug.LogWarning("Audio source null!");
                return false;
            }

            this.state = State.PLAYING;
            
            this.audioSource.clip = track;
            this.audioSource.time = 0;

            this.audioSource.Play();
            this.OnTrackStarted?.Invoke(track);
            return true;
        }

        [Button, HideInEditorMode]
        [GUIColor(1f, 0.83f, 0f)]
        public bool Pause()
        {
            if (this.state != State.PLAYING)
            {
                Debug.LogWarning("Music is not playing!");
                return false;
            }

            if (this.audioSource == null)
            {
                Debug.LogWarning("Audio source null!");
                return false;
            }
            
            this.state = State.PAUSED;
            
            this.audioSource.Pause();
            this.OnTrackPaused?.Invoke(this.audioSource.clip);
            return true;
        }

        [Button, HideInEditorMode]
        [GUIColor(1f, 0.83f, 0f)]
        public bool Resume()
        {
            if (this.state != State.PAUSED)
            {
                Debug.LogWarning("Music is not paused!");
                return false;
            }
            
            if (this.audioSource == null)
            {
                Debug.LogWarning("Audio source null!");
                return false;
            }

            this.state = State.PLAYING;
            this.audioSource.UnPause();
            this.OnTrackResumed?.Invoke(this.audioSource.clip);
            return true;
        }

        [Button, HideInEditorMode]
        [GUIColor(1f, 0.83f, 0f)]
        public bool Stop()
        {
            if (this.state == State.IDLE)
            {
                Debug.LogWarning("Music is not playing!");
                return false;
            }
            
            if (this.audioSource == null)
            {
                Debug.LogWarning("Audio source null!");
                return false;
            }

            AudioClip currentTrack = this.audioSource.clip;

            this.state = State.IDLE;
            this.audioSource.Stop();
            this.audioSource.clip = null;
            
            this.OnTrackStopped?.Invoke(currentTrack);
            return true;
        }

        public bool Replay(AudioClip track)
        {
            this.Stop();
            return this.Play(track);
        }

        private void CheckTrackFinished()
        {
            if (this.audioSource.time < this.audioSource.clip.length)
            {
                return;
            }

            this.state = State.FINISHED;
            this.audioSource.Stop();
            this.OnTrackFinsihed?.Invoke(this.audioSource.clip);
        }

        private void SetVolume(float volume)
        {
            if (this.audioSource == null)
            {
                return;
            }
            
            volume = Mathf.Clamp01(volume);

            if (Mathf.Approximately(volume, this.audioSource.volume))
            {
                return;
            }

            this.audioSource.volume = volume;
            this.OnVolumeChanged?.Invoke(volume);
        }

        private void SetMute(bool mute)
        {
            if (this.audioSource == null)
            {
                return;
            }
            
            if (this.audioSource.mute == mute)
            {
                return;
            }

            this.audioSource.mute = mute;
            this.OnMuteChanged?.Invoke(mute);
        }

        private float GetCurrentProgress()
        {
            if (this.state == State.FINISHED)
            {
                return 1;
            }

            if (this.state == State.IDLE)
            {
                return 0.0f;
            }

            if (this.audioSource == null || this.audioSource.clip == null)
            {
                return 0.0f;
            }

            return this.audioSource.time / this.audioSource.clip.length;
        }

        private void SetCurrentProgress(float progress)
        {
            if (this.state is State.IDLE or State.FINISHED)
            {
                return;
            }

            progress = Mathf.Clamp01(progress);

            if (this.audioSource == null || this.audioSource.clip == null)
            {
                this.audioSource.time = progress * this.audioSource.clip.length;
            }
        }

        private float GetCurrentTime()
        {
            if (this.state is State.IDLE or State.FINISHED)
            {
                return 0.0f;
            }

            return this.audioSource != null ? this.audioSource.time : 0;
        }

        private void SetCurrentTime(float time)
        {
            if (this.state is State.IDLE or State.FINISHED)
            {
                return;
            }

            if (this.audioSource == null)
            {
                return;
            }

            var audioClip = this.audioSource.clip;
            if (audioClip == null)
            {
                return;
            }

            this.audioSource.time = Mathf.Clamp(time, 0, audioClip.length - 0.1f);
        }

        private AudioClip GetCurrentTrack()
        {
            return this.audioSource != null ? this.audioSource.clip : null;
        }

        private bool GetMute()
        {
            if (this.audioSource != null)
            {
                return audioSource.mute;
            }

            return true;
        }

        private float GetVolume()
        {
            return this.audioSource != null ? this.audioSource.volume : 0;
        }

        private float GetPitch()
        {
            return this.audioSource != null ? this.audioSource.pitch : 0;
        }

        private void SetPitch(float value)
        {
            if (this.audioSource != null && !Mathf.Approximately(this.audioSource.pitch, value))
            {
                this.audioSource.pitch = value;
                this.OnPitchChanged?.Invoke(value);
            }
        }

        private float GetCurrentTrackLength()
        {
            AudioClip clip = this.audioSource.clip;
            if (clip != null)
            {
                return clip.length;
            }

            return 0;
        }
    }
}