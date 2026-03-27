using System.Linq;
using BaseModule;
using InventoryModule;

namespace BuildingModule
{
    public class StorageService : IStorageService
    {
        private readonly BuildingCatalog _buildingCatalog;
        private readonly IInventoryManager _inventoryManager;
        private readonly IBaseLevelService _baseLevelService;

        public StorageService(
            BuildingCatalog buildingCatalog,
            IInventoryManager inventoryManager,
            IBaseLevelService baseLevelService)
        {
            _buildingCatalog = buildingCatalog;
            _inventoryManager = inventoryManager;
            _baseLevelService = baseLevelService;
        }

        public bool CanBuy(string id)
        {
            if (!_buildingCatalog.TryGetConfig(id, out var config))
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

        public bool Buy(string id)
        {
            if (!_buildingCatalog.TryGetConfig(id, out var config))
                return false;

            if (!CanBuy(id))
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
                    return false;
            }

            _baseLevelService.AddPoints(config.BasePoints);
            return true;
        }
    }
}