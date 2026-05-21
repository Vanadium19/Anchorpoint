using UnityEngine;

namespace AudioModule
{
    public partial interface IAudioSystem
    {
        bool PlayEvent(string eventId, float maxFrequency = 0);
        bool PlayEvent(string eventId, out AudioEventHandle handle, float maxFrequency = 0);
        
        bool PlayEvent(string eventId, Vector3 position, float maxFrequency = 0);
        bool PlayEvent(string eventId, Vector3 position, out AudioEventHandle handle, float maxFrequency = 0);
        bool PlayEvent(string eventId, Vector3 position, Quaternion rotation, float maxFrequency = 0);
        bool PlayEvent(string eventId, Vector3 position, Quaternion rotation, out AudioEventHandle handle, float maxFrequency = 0);
        bool PlayEvent(string eventId, Transform pivot, float maxFrequency = 0);
        bool PlayEvent(string eventId, Transform pivot, out AudioEventHandle handle, float maxFrequency = 0);

        bool TryCreateEvent(string eventId, out AudioEventHandle result);
        bool TryCreateEvent(string eventId, Vector3 position, Quaternion rotation, out AudioEventHandle result);
        AudioEventHandle CreateEvent(string eventId);
        AudioEventHandle CreateEvent(string eventId, Vector3 position, Quaternion rotation);
      
        bool DisposeEvent(string eventId);
        bool DisposeEvent(string eventId, float fadeoutTime);
        bool DisposeEvent(string eventId, AnimationCurve curve, float fadeoutTime);
        
        void DisposeEvents(string eventId);
        void DisposeEvents(string eventId, float fadeoutTime);
        void DisposeEvents(string eventId, AnimationCurve curve, float fadeoutTime);
        
        bool FindEvent(string eventId, out AudioEventHandle result);
        int FindEventsNonAlloc(string eventId, AudioEventHandle[] results);
    }
}