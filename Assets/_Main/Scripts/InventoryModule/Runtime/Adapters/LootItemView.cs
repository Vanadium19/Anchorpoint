using ComponentsModule;
using UnityEngine;
using Zenject;

namespace InventoryModule
{
    public class LootItemView : MonoBehaviour, IInteractable
    {
        [SerializeField] private ItemDataSo itemData;
        [SerializeField] private int amount = 1;

        [Header("Collect Feedback")]
        [SerializeField] private GameObject collectEffect;
        [SerializeField] private float collectEffectLifetime = 2f;
        [SerializeField] private AudioSource collectSound;

        private ItemTable _itemTable;
        private bool _isPlayerDeathLoot;

        private LootPickupService _pickupService;

        [Inject]
        private void Construct(LootPickupService pickupService)
        {
            _pickupService = pickupService;
        }

        public ItemDataSo ItemData => itemData;
        public int Amount => amount;
        public ItemTable ItemTable => _itemTable;
        public bool IsPlayerDeathLoot => _isPlayerDeathLoot;

        public string DisplayName => itemData != null ? $"{itemData.DisplayName} x{amount}" : string.Empty;

        public string HintKey => ComponentsModule.InteractionHintKeys.PickUp;

        public void SetItemTable(ItemTable item)
        {
            _itemTable = item;
            itemData = item.ItemDataSo;
            amount = item.StackCount;
        }

        public bool CanInteract(Transform interactor) => true;

        public void Interact(Transform interactor)
        {
            if (_pickupService == null || !_pickupService.Collect(this))
                return;

            PlayCollectEffects();
            Destroy(gameObject);
        }

        public void SetPlayerDeathLoot(bool isPlayerDeathLoot) =>
            _isPlayerDeathLoot = isPlayerDeathLoot;
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
