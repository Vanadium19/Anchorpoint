using System;

namespace EvacuationModule
{
    public interface IEvacuationService
    {
        event Action Canceled;
        event Action Completed;

        event Action<float> TimerChanged;

        void ChangeZoneState(bool isInside);
    }
}