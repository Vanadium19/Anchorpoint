using UnityEngine;

namespace AudioModule
{
    public interface IUISoundPlayer
    {
        bool PlayOneShot(string soundKey);
        
        void PlayOneShot(AudioClip sound);
    }
}