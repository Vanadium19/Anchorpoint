using System.Linq;
using System.Threading;
using InventoryModule;
using BaseModule;

namespace BuildingModule
{
    public class StorageService : IStorageService
    {
        private readonly BuildingCatalog _buildingCatalog;
        private readonly InventoryModel _inventory;
        private readonly IInventoryService _inventoryService;
        private readonly IBaseLevelService _baseLevelService;

        public StorageService(InventoryModel inventory,
            BuildingCatalog buildingCatalog,
            IInventoryService inventoryService,
            IBaseLevelService baseLevelService)
        {
            _inventory = inventory;
            _buildingCatalog = buildingCatalog;
            _inventoryService = inventoryService;
            _baseLevelService = baseLevelService;
        }

        public bool CanBuy(string id)
        {
            if (!_buildingCatalog.TryGetConfig(id, out var config))
                return false;

            foreach (var itemToCount in config.Price.Values)
            {
                var resource = _inventory.Items.FirstOrDefault(item => item.Id == itemToCount.ItemDefinition.Id);

                if (resource == null)
                    return false;

                if (resource.Amount < itemToCount.Count)
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

            foreach (var itemToCount in config.Price.Values)
            {
                var resource = _inventory.Items.First(item => item.Id == itemToCount.ItemDefinition.Id);
                resource.Amount -= itemToCount.Count;
                _inventoryService.UpdateInventory();

                if (resource.Amount <= 0)
                    _inventoryService.RemoveItemAsync(resource, CancellationToken.None);
            }

            _baseLevelService.AddPoints(config.BasePoints);
            return true;
        }
    }
}