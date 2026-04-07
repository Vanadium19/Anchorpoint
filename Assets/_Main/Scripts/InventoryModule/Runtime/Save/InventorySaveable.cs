using System;
using System.Collections.Generic;
using UnityEngine;
using SaveModule;
using Sirenix.Serialization;

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

        public string CreateMementoJson()
        {
            var inventoryJson = SerializeInventory();
            var equipmentJson = SerializeEquipment();

            var combined = new Dictionary<string, string>
            {
                [InventoryKey] = inventoryJson,
                [EquipmentKey] = equipmentJson
            };

            var bytes = SerializationUtility.SerializeValue(combined, DataFormat.JSON);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        public void RestoreMementoFromJson(string json)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            var combined = SerializationUtility.DeserializeValue<Dictionary<string, string>>(bytes, DataFormat.JSON);

            if (combined == null)
            {
                Debug.LogError("[InventorySaveable] Failed to deserialize memento");
                return;
            }

            if (combined.TryGetValue(InventoryKey, out var invJson))
                RestoreInventory(invJson);

            if (combined.TryGetValue(EquipmentKey, out var eqJson))
                RestoreEquipment(eqJson);
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

            var bytes = SerializationUtility.SerializeValue(memento, DataFormat.JSON);
            return System.Text.Encoding.UTF8.GetString(bytes);
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

            var bytes = SerializationUtility.SerializeValue(memento, DataFormat.JSON);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        private void RestoreInventory(string json)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            var memento = SerializationUtility.DeserializeValue<InventoryMemento>(bytes, DataFormat.JSON);

            if (memento == null)
            {
                Debug.LogError("[InventorySaveable] Failed to deserialize inventory memento");
                return;
            }

            if (_inventoryManager.MainGrid == null)
            {
                Debug.LogWarning("[InventorySaveable] MainGrid is null, cannot restore inventory");
                return;
            }

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
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            var memento = SerializationUtility.DeserializeValue<EquipmentMemento>(bytes, DataFormat.JSON);

            if (memento == null)
            {
                Debug.LogError("[InventorySaveable] Failed to deserialize equipment memento");
                return;
            }

            if (_slotService == null)
            {
                Debug.LogWarning("[InventorySaveable] SlotService is null");
                return;
            }

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
