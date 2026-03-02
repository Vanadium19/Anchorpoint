using UnityEngine;

namespace InventoryModule
{
    public class LootItemView : MonoBehaviour
    {
        [SerializeField] private ItemDataSo itemData;
        [SerializeField] private int amount = 1;
        [SerializeField] private GameObject collectEffect;
        [SerializeField] private AudioSource collectSound;

        public ItemDataSo ItemData => itemData;
        public int Amount => amount;
        public ItemTable ItemTable { get; private set; }

        public void Initialize(ItemDataSo data, int count)
        {
            itemData = data;
            amount = count;
        }

        public void SetItemTable(ItemTable item)
        {
            ItemTable = item;
            itemData = item.ItemDataSo;
            amount = item.StackCount;
        }

        public void PlayCollectEffects()
        {
            if (collectEffect != null)
                Instantiate(collectEffect, transform.position, Quaternion.identity);

            if (collectSound != null && collectSound.clip != null)
                AudioSource.PlayClipAtPoint(collectSound.clip, transform.position);
        }

        public void SetAmount(int count)
        {
            amount = count;
        }
    }
}
