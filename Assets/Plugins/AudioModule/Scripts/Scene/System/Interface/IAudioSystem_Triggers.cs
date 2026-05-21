using System;

namespace AudioModule
{
    public partial interface IAudioSystem
    {
        void SetTrigger(string triggerId, Action trigger);
        void ResetTrigger(string triggerId);
    }
}