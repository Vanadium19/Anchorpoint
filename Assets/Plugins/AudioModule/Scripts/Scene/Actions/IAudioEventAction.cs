namespace AudioModule
{
    public interface IAudioEventAction
    {
        void Invoke(in AudioSourceEvent evt);
    }
}