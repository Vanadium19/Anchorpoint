using UnityEngine;
using Zenject;

namespace InventoryModule
{
    public class LootItemView : MonoBehaviour, ILootable
    {
        [SerializeField] private ItemName itemName;
        [SerializeField] private int amount = 1;

        private ItemCatalog _catalog;
        private ItemConfig _cachedConfig;

        [Inject]
        public void Construct(ItemCatalog catalog)
        {
            _catalog = catalog;
        }

        public ItemConfig Config
        {
            get
            {
                if (_cachedConfig == null && _catalog != null)
                    _catalog.TryGetConfig(itemName, out _cachedConfig);
                return _cachedConfig;
            }
        }

        public int Amount => amount;

        public void Collect() => Destroy(gameObject);
    }
}