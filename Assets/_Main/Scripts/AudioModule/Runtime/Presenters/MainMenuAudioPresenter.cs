using System;
using Zenject;

namespace AudioModule
{
    public class MainMenuAudioPresenter : IInitializable
    {
        private readonly IAudioSystem _audioSystem;

        public MainMenuAudioPresenter(IAudioSystem audioSystem)
        {
            _audioSystem = audioSystem;
        }

        public void Initialize()
        {
            _audioSystem.PlayEvent(MenuBankAPI.AmbientEvent);
        }
    }
}