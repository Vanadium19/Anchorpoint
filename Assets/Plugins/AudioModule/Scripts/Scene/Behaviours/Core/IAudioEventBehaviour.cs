namespace AudioModule
{
    public interface IAudioEventBehaviour
    {
        void OnStart(in AudioSourceEvent evt);

        void OnUpdate(in AudioSourceEvent evt);

        void OnStop(in AudioSourceEvent evt);

        void OnReset(in AudioSourceEvent evt);
    }
}