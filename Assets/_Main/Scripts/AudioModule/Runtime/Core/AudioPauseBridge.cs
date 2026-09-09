using System;
using BaseModule;
using Zenject;

namespace AudioModule
{
    /// <summary>Silences everything the scene is playing while the game is paused and brings it back on resume.</summary>
    /// <remarks>
    /// The audio system pauses the events that are sounding at that moment, so a looping fire or a
    /// Geiger counter goes quiet with the game; a sound started later — a UI click on the pause
    /// screen — is unaffected.
    /// </remarks>
    public class AudioPauseBridge : IPausable, IInitializable, IDisposable
    {
        private readonly IAudioSystem _audioSystem;
        private readonly IPauseManager _pauseManager;

        /// <summary>Creates the bridge between the scene pause manager and the audio system.</summary>
        public AudioPauseBridge(IAudioSystem audioSystem, IPauseManager pauseManager)
        {
            _audioSystem = audioSystem;
            _pauseManager = pauseManager;
        }

        /// <summary>Registers with the pause manager.</summary>
        public void Initialize() => _pauseManager.Register(this);

        /// <summary>Unregisters from the pause manager.</summary>
        public void Dispose() => _pauseManager.Unregister(this);

        /// <inheritdoc/>
        public void SetPaused(bool isPaused)
        {
            if (isPaused)
                _audioSystem.Pause();
            else
                _audioSystem.Resume();
        }
    }
}
