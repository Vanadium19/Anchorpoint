namespace AudioModule
{
    public interface IAudioSettingsMemento
    {
        AudioSettingsData CreateSnapshot();
        void SetSnapshot(AudioSettingsData data);
    }
}
