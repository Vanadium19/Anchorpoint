using UnityEngine;
using UnityEngine.Events;

namespace AudioModule
{
    [AddComponentMenu("Audio/Audio Callback Listener")]
    public sealed class AudioCallbackListener : MonoBehaviour
    {
        [SerializeField]
        private string callback;

        [SerializeField]
        private UnityEvent action;

        private void OnEnable()
        {
            if (AudioSystem.Instance)
                AudioSystem.Instance.SubscribeOnCallback(this.callback, this.action.Invoke);
        }

        private void OnDisable()
        {
            if (AudioSystem.Instance)
                AudioSystem.Instance.UnsubscribeFromCallback(this.callback, this.action.Invoke);
        }
    }
}