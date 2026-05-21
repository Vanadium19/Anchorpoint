using UnityEngine;

namespace AudioModule
{
    public class MenuMusic : MonoBehaviour
    {
        private AudioEventHandle _handle;

        private void Awake()
        {
            var audioSystem = AudioSystem.Resolve(this);
            audioSystem.PlayEvent(MenuBankAPI.AmbientEvent, out _handle);
        }

        private void OnDestroy()
        {
            _handle.Dispose();
        }
    }
}