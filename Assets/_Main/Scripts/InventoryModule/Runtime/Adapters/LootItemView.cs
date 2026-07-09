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

        public ItemDataSo ItemData => itemData;
        public int Amount => amount;
        public ItemTable ItemTable => _itemTable;

        public void SetItemTable(ItemTable item)
        {
            _itemTable = item;
            itemData = item.ItemDataSo;
            amount = item.StackCount;
        }

        public void SetAmount(int newAmount)
        {
            amount = Mathf.Max(0, newAmount);

            if (_itemTable != null)
                _itemTable.StackCount = amount;
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
