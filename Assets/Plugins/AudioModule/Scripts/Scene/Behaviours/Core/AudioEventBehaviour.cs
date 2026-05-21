namespace AudioModule
{
    public class AudioEventBehaviour : IAudioEventBehaviour
    {
        public virtual void OnStart(in AudioSourceEvent evt) { }

        public virtual void OnUpdate(in AudioSourceEvent evt) { }

        public virtual void OnStop(in AudioSourceEvent evt) { }

        public virtual void OnReset(in AudioSourceEvent evt) { }
    }
}