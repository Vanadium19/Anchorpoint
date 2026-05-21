using System;
using System.Collections.Generic;
using UnityEngine;

namespace AudioModule
{
    public sealed class MusicPlaylist
    {
        public event Action OnStarted;
        public event Action OnStopped;
        public event Action OnFinished;
        public event Action<AudioClip> OnTrackChanged;

        public bool IsPlaying => _isPlaying;
        public AudioClip CurrentClip => _clipEnumerator.Current;
        public IEnumerator<AudioClip> ClipEnumerator => _clipEnumerator;

        private readonly MusicPlayer _musicPlayer;
        private readonly IEnumerator<AudioClip> _clipEnumerator;
        private bool _isPlaying;

        public MusicPlaylist(MusicPlayer musicPlayer, IEnumerator<AudioClip> clipEnumerator)
        {
            _musicPlayer = musicPlayer;
            _clipEnumerator = clipEnumerator;
        }
        
        public void Play()
        {
            if (_isPlaying)
            {
                Debug.LogWarning("Playlist is already started!");
                return;
            }

            _clipEnumerator.Reset();

            if (!_clipEnumerator.MoveNext())
            {
                Debug.LogWarning("Track list is empty");
                return;
            }

            _isPlaying = true;
            this.OnStarted?.Invoke();

            _musicPlayer.OnTrackFinsihed += this.OnTrackFinished;
            _musicPlayer.Stop();
            _musicPlayer.Play(_clipEnumerator.Current);
            
            this.OnTrackChanged?.Invoke(_clipEnumerator.Current);
        }

        public void Stop()
        {
            if (!_isPlaying)
            {
                Debug.LogWarning("Playlist is not started!");
                return;
            }

            _isPlaying = false;
            _clipEnumerator.Dispose();

            _musicPlayer.Stop();
            _musicPlayer.OnTrackFinsihed -= this.OnTrackFinished;
            this.OnStopped?.Invoke();
        }

        public void Restart()
        {
            this.Stop();
            this.Play();
        }
        
        private void OnTrackFinished(AudioClip track)
        {
            if (_clipEnumerator.MoveNext())
            {
                _musicPlayer.Play(_clipEnumerator.Current);
                this.OnTrackChanged?.Invoke(_clipEnumerator.Current);
            }
            else
            {
                _isPlaying = false;
                this.OnFinished?.Invoke();
            }
        }
    }
}