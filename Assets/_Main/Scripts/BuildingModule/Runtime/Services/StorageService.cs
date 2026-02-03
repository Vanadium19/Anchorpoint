using System.Linq;
using InventoryModule;

namespace BuildingModule
{
    public class StorageService : IStorageService
    {
        private readonly BuildingCatalog _buildingCatalog;

        //FIXME: Нужнен ефактор инвентаря
        private readonly InventoryModel _inventory;
        private readonly IInventoryService _inventoryService;

        public StorageService(InventoryModel inventory,
            BuildingCatalog buildingCatalog,
            IInventoryService inventoryService)
        {
            _inventory = inventory;
            _buildingCatalog = buildingCatalog;
            _inventoryService = inventoryService;
        }

        public bool CanBuy(BuildingName name)
        {
            if (!_buildingCatalog.TryGetConfig(name, out var config))
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

        public bool Buy(BuildingName name)
        {
            if (!_buildingCatalog.TryGetConfig(name, out var config))
                return false;

            if (!CanBuy(name))
                return false;

            foreach (var itemToCount in config.Price.Values)
            {
                var resource = _inventory.Items.First(item => item.Id == itemToCount.ItemDefinition.Id);
                resource.AddAmount(-itemToCount.Count);
                _inventoryService.UpdateInventory();

                if (resource.Amount <=0)
                    _inventoryService.RemoveItem(resource);
            }

            return true;
        }
    }
}