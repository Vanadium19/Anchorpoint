using Sirenix.OdinInspector;
using UnityEngine;

namespace AudioModule
{
    [AddComponentMenu("Audio/Audio Event Holder")]
    public sealed class AudioEventHolder : MonoBehaviour
    {
        [SerializeField]
        private AudioEventKey _key;

        [SerializeField]
        private float _maxFrequency;

        [SerializeField]
        private Transform _point;

        [Header("Unity Callbacks")]
        [SerializeField]
        private bool _playOnStart = true;

        [SerializeField]
        private bool _disposeOnDestroy = true;

        [Button, GUIColor(1f, 0.83f, 0f), HideInEditorMode]
        public void PlayEvent()
        {
            if (AudioSystem.Instance)
                AudioSystem.Instance.PlayEvent(_key, _point.position, _point.rotation, _maxFrequency);
        }

        [Button, GUIColor(1f, 0.83f, 0f), HideInEditorMode]
        public void DisposeEvent()
        {
            if (AudioSystem.Instance)
                AudioSystem.Instance.DisposeEvent(_key);
        }

        private void Awake()
        {
            if (_playOnStart)
                this.PlayEvent();
        }

        private void OnDestroy()
        {
            if (_disposeOnDestroy)
                this.DisposeEvent();
        }

        private void Reset()
        {
            _point = this.transform;
        }
    }
}