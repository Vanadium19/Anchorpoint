using System.Collections.Generic;
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

        public BuildPriceInfo GetPriceInfo(string id)
        {
            if (!_buildingCatalog.TryGetConfig(id, out var config))
                return null;

            var price = config.Price;
            var info = new BuildPriceInfo
            {
                BuildingName = config.DisplayName
            };

            if (price?.Values == null)
            {
                info.AvailableCount = int.MaxValue;
                info.Items = new List<PriceItemInfo>();
                return info;
            }

            int minAvailable = int.MaxValue;

            foreach (var itemToCount in price.Values)
            {
                if (itemToCount.ItemData == null)
                    continue;

                int available = _inventoryManager.GetItemCount(itemToCount.ItemData);
                int canAfford = itemToCount.Count > 0 ? available / itemToCount.Count : int.MaxValue;

                if (canAfford < minAvailable)
                    minAvailable = canAfford;

                info.Items.Add(new PriceItemInfo
                {
                    ItemData = itemToCount.ItemData,
                    Available = available,
                    Cost = itemToCount.Count
                });
            }

            info.AvailableCount = minAvailable == int.MaxValue ? 0 : minAvailable;
            return info;
        }
    }
}