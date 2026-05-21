using UnityEngine;

namespace AudioModule
{
    public interface IClipProvider
    {
        AudioClip Value { get; }
        
        float MaxLength { get; }
    }
}