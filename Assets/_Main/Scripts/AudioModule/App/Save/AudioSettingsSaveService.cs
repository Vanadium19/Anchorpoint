using System;
using Newtonsoft.Json;
using SaveModule;
using Zenject;

namespace AudioModule
{
    public class AudioSettingsSaveService : IInitializable, IDisposable, ISaveable
    {
        private readonly IAudioSettingsService _audioSettingsService;
        private readonly IAudioSettingsMemento _audioSettingsMemento;
        private readonly IGameSaveLoader _gameSaveLoader;

        public string SaveKey => nameof(AudioSettingsSaveService);

        public AudioSettingsSaveService(IAudioSettingsService audioSettingsService,
            IAudioSettingsMemento audioSettingsMemento,
            [Inject(Id = GameSaveLoaderIds.Settings)] IGameSaveLoader gameSaveLoader)
        {
            _audioSettingsService = audioSettingsService;
            _audioSettingsMemento = audioSettingsMemento;
            _gameSaveLoader = gameSaveLoader;
        }

        public void Initialize()
        {
            _gameSaveLoader.RegisterSaveable(this);
            _gameSaveLoader.Load();
            _audioSettingsService.VolumeChanged += OnVolumeChanged;
        }

        public void Dispose()
        {
            _audioSettingsService.VolumeChanged -= OnVolumeChanged;
            _gameSaveLoader.Save();
            _gameSaveLoader.UnregisterSaveable(this);
        }

        public string CreateMemento()
        {
            var snapshot = _audioSettingsMemento.CreateSnapshot();
            return JsonConvert.SerializeObject(snapshot);
        }

        public void RestoreMemento(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                return;

            var snapshot = JsonConvert.DeserializeObject<AudioSettingsData>(data);

            if (snapshot == null)
                return;

            _audioSettingsMemento.SetSnapshot(snapshot);
        }

        private void OnVolumeChanged(AudioMixerChannel channel, float volume) => _gameSaveLoader.Save();
    }
}