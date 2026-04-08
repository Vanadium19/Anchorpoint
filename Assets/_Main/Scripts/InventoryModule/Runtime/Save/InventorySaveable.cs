using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SaveModule;

namespace InventoryModule
{
    public class InventorySaveable : ISaveable
    {
        private const string InventoryKey = "inventory";
        private const string EquipmentKey = "equipment";

        private readonly IInventoryManager _inventoryManager;
        private readonly IEquipmentSlotService _slotService;
        private readonly ItemCatalog _itemCatalog;

        public string SaveKey => InventoryKey;

        public InventorySaveable(
            IInventoryManager inventoryManager,
            IEquipmentSlotService slotService,
            ItemCatalog itemCatalog)
        {
            _inventoryManager = inventoryManager;
            _slotService = slotService;
            _itemCatalog = itemCatalog;
        }

        public string CreateMemento()
        {
            var inventoryData = SerializeInventory();
            var equipmentData = SerializeEquipment();

            var combined = new Dictionary<string, string>
            {
                [InventoryKey] = inventoryData,
                [EquipmentKey] = equipmentData
            };

            return JsonConvert.SerializeObject(combined);
        }

        public void RestoreMemento(string data)
        {
            var combined = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);

            if (combined == null)
                return;

            if (combined.TryGetValue(InventoryKey, out var invData))
                RestoreInventory(invData);

            if (combined.TryGetValue(EquipmentKey, out var eqData))
                RestoreEquipment(eqData);
        }

        private string SerializeInventory()
        {
            var memento = new InventoryMemento();
            var mainGrid = _inventoryManager.MainGrid;

            if (mainGrid != null)
            {
                memento.GridWidth = mainGrid.Width;
                memento.GridHeight = mainGrid.Height;

                var items = mainGrid.GetAllItems();
                foreach (var item in items)
                {
                    memento.Items.Add(ItemSerializer.Serialize(item));
                }
            }

            return JsonConvert.SerializeObject(memento);
        }

        private string SerializeEquipment()
        {
            var memento = new EquipmentMemento();

            if (_slotService != null)
            {
                foreach (var slot in _slotService.GetAllSlots())
                {
                    if (slot == null || slot.EquippedItem == null)
                        continue;

                    var slotMemento = new EquipmentSlotMemento
                    {
                        SlotType = slot.SlotType.ToString(),
                        Item = ItemSerializer.Serialize(slot.EquippedItem)
                    };

                    memento.Slots.Add(slotMemento);
                }
            }

            return JsonConvert.SerializeObject(memento);
        }

        private void RestoreInventory(string json)
        {
            var memento = JsonConvert.DeserializeObject<InventoryMemento>(json);

            if (memento == null)
                return;

            if (_inventoryManager.MainGrid == null)
                return;

            _inventoryManager.ClearInventory();

            if (memento.Items != null)
            {
                foreach (var itemMemento in memento.Items)
                {
                    ItemSerializer.RestoreItem(_inventoryManager.MainGrid, itemMemento, _itemCatalog);
                }
            }
        }

        private void RestoreEquipment(string json)
        {
            var memento = JsonConvert.DeserializeObject<EquipmentMemento>(json);

            if (memento == null)
                return;

            if (_slotService == null)
                return;

            if (memento.Slots != null)
            {
                foreach (var slotMemento in memento.Slots)
                {
                    if (string.IsNullOrEmpty(slotMemento.SlotType))
                        continue;

                    if (!Enum.TryParse<EquipmentSlotType>(slotMemento.SlotType, out var slotType))
                        continue;

                    var slot = _slotService.GetSlot(slotType);

                    if (slot == null)
                        continue;

                    var itemData = _itemCatalog.GetByName(slotMemento.Item.ItemDataName);

                    if (itemData == null)
                        continue;

                    var item = ItemSerializer.Deserialize(slotMemento.Item, itemData);

                    slot.TryEquip(item);
                    ItemSerializer.RestoreNestedContainers(item, slotMemento.Item, _itemCatalog);
                }
            }
        }
    }
}
