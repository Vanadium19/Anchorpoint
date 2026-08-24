using ComponentsModule;
using UnityEngine;
using Zenject;

namespace InventoryModule
{
    public class LootItemView : MonoBehaviour, IInteractable
    {
        [SerializeField] private ItemDataSo itemData;
        [SerializeField] private int amount = 1;
        [SerializeField] private GameObject collectEffect;
        [SerializeField] private AudioSource collectSound;

        private ItemTable _itemTable;

        private LootPickupService _pickupService;

        [Inject]
        private void Construct(LootPickupService pickupService)
        {
            _pickupService = pickupService;
        }

        public ItemDataSo ItemData => itemData;
        public int Amount => amount;
        public ItemTable ItemTable => _itemTable;

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

        public void PlayCollectEffects()
        {
            if (collectEffect != null)
                Instantiate(collectEffect, transform.position, Quaternion.identity);

            if (collectSound != null && collectSound.clip != null)
                AudioSource.PlayClipAtPoint(collectSound.clip, transform.position);
        }
    }
}
