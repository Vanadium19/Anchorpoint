using UnityEngine;

namespace InventoryModule
{
    public class LootItemView : MonoBehaviour
    {
        [SerializeField] private ItemDataSo itemData;
        [SerializeField] private int amount = 1;
        [SerializeField] private GameObject collectEffect;
        [SerializeField] private AudioSource collectSound;

        private ItemTable _itemTable;
        private bool _isPlayerDeathLoot;

        public ItemDataSo ItemData => itemData;
        public int Amount => amount;
        public ItemTable ItemTable => _itemTable;
        public bool IsPlayerDeathLoot => _isPlayerDeathLoot;

        public void SetItemTable(ItemTable item)
        {
            _itemTable = item;
            itemData = item.ItemDataSo;
            amount = item.StackCount;
        }

        public void SetPlayerDeathLoot(bool isPlayerDeathLoot) =>
            _isPlayerDeathLoot = isPlayerDeathLoot;

        public void PlayCollectEffects()
        {
            if (collectEffect != null)
                Instantiate(collectEffect, transform.position, Quaternion.identity);

            if (collectSound != null && collectSound.clip != null)
                AudioSource.PlayClipAtPoint(collectSound.clip, transform.position);
        }
    }
}
