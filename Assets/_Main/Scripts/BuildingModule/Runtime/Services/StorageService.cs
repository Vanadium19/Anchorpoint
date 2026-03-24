using System.Linq;
using InventoryModule;

namespace BuildingModule
{
    public class StorageService : IStorageService
    {
        private readonly BuildingCatalog _buildingCatalog;
        private readonly IInventoryManager _inventoryManager;

        public StorageService(
            BuildingCatalog buildingCatalog,
            IInventoryManager inventoryManager)
        {
            _buildingCatalog = buildingCatalog;
            _inventoryManager = inventoryManager;
        }

        public bool CanBuy(BuildingName name)
        {
            if (!_buildingCatalog.TryGetConfig(name, out var config))
                return false;

            var price = config.Price;
            if (price?.Values == null)
                return true;

            foreach (var itemToCount in price.Values)
            {
                if (itemToCount.ItemData == null)
                    continue;

                int available = _inventoryManager.GetItemCount(itemToCount.ItemData);
                if (available < itemToCount.Count)
                    return false;
            }

            return true;
        }

        public bool Buy(BuildingName name)
        {
            if (!_buildingCatalog.TryGetConfig(name, out var config))
                return false;

            if (!CanBuy(name))
                return false;

            var price = config.Price;
            if (price?.Values == null)
                return true;

            foreach (var itemToCount in price.Values)
            {
                if (itemToCount.ItemData == null)
                    continue;

                bool removed = _inventoryManager.TryRemoveItems(itemToCount.ItemData, itemToCount.Count);
                if (!removed)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
