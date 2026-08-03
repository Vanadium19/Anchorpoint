using UnityEngine;

namespace InventoryModule
{
    public class LootItemView : MonoBehaviour
    {
        [SerializeField] private ItemDataSo itemData;
        [SerializeField] private int amount = 1;

        [Header("Collect Feedback")]
        [SerializeField] private GameObject collectEffect;
        [SerializeField] private float collectEffectLifetime = 2f;
        [SerializeField] private AudioSource collectSound;

        private ItemTable _itemTable;

        public ItemDataSo ItemData => itemData;
        public int Amount => amount;
        public ItemTable ItemTable => _itemTable;

        public void SetItemTable(ItemTable item)
        {
            _itemTable = item;
            itemData = item.ItemDataSo;
            amount = item.StackCount;
        }

        public void PlayCollectEffects()
        {
            PlayCollectEffect();
            PlayCollectSound();
        }

        private void PlayCollectEffect()
        {
            if (collectEffect == null)
                return;

            var effect = Instantiate(collectEffect, transform.position, Quaternion.identity);
            Destroy(effect, collectEffectLifetime);
        }

        private void PlayCollectSound()
        {
            if (collectSound == null || collectSound.clip == null)
                return;

            var audioObject = new GameObject("CollectSound");
            audioObject.transform.position = transform.position;

            var audioSource = audioObject.AddComponent<AudioSource>();
            CopyAudioSettings(collectSound, audioSource);
            audioSource.Play();

            Destroy(audioObject, collectSound.clip.length + 0.1f);
        }

        private static void CopyAudioSettings(AudioSource source, AudioSource target)
        {
            target.clip = source.clip;
            target.outputAudioMixerGroup = source.outputAudioMixerGroup;
            target.volume = source.volume;
            target.pitch = source.pitch;
            target.spatialBlend = source.spatialBlend;
            target.rolloffMode = source.rolloffMode;
            target.minDistance = source.minDistance;
            target.maxDistance = source.maxDistance;
            target.playOnAwake = false;
        }
    }
}
