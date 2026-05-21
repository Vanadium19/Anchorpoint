using Sirenix.OdinInspector;
using UnityEngine;

namespace AudioModule
{
    [AddComponentMenu("Audio/UI Sound Player")]
    public sealed class UISoundPlayer : MonoBehaviour, IUISoundPlayer
    {
        public static UISoundPlayer Instance { get; private set; }

        [SerializeField]
        private AudioSource audioSource;

        [SerializeField]
        private UISoundCatalog catalog;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        private void Reset()
        {
            this.audioSource = this.GetComponent<AudioSource>();
        }

        [Title("Methods")]
        [Button, GUIColor(1f, 0.83f, 0f), HideInEditorMode]
        public bool PlayOneShot([UISoundKey] string soundKey)
        {
            if (string.IsNullOrEmpty(soundKey))
            {
                Debug.LogWarning("Sound key is empty!");
                return false;
            }

            if (!this.catalog.TryGetSound(soundKey, out AudioClip sound))
            {
                Debug.LogWarning($"Sound with key {soundKey} is not found!");
                return false;
            }
            
            this.audioSource.PlayOneShot(sound);
            return true;
        }

        public void PlayOneShot(AudioClip sound)
        {
            this.audioSource.PlayOneShot(sound);
        }
        
        public void SetCatalog(UISoundCatalog catalog)
        {
            this.catalog = catalog;
        }
    }
}