using System.Collections.Generic;
using Zenject;

namespace InventoryModule
{
    public sealed class EquipmentSlotService : IEquipmentSlotService
    {
        private readonly List<EquipmentSlot> _slots = new();
        private readonly DiContainer _container;

        private IInventoryManager _inventoryManager;

        public EquipmentSlotService(DiContainer container)
        {
            _container = container;
        }

        private IInventoryManager InventoryManager
        {
            get
            {
                if (_inventoryManager == null && _container != null)
                    _inventoryManager = _container.TryResolve<IInventoryManager>();

                return _inventoryManager;
            }
        }

        public void RegisterSlot(EquipmentSlot slot)
        {
            if (slot != null && !_slots.Contains(slot))
                _slots.Add(slot);
        }

        public void UnregisterSlot(EquipmentSlot slot)
        {
            if (slot != null)
                _slots.Remove(slot);
        }

        public IReadOnlyList<EquipmentSlot> GetAllSlots() => _slots;

        public EquipmentSlot GetSlotForItem(ItemTable item)
        {
            if (item == null)
                return null;

            foreach (var slot in _slots)
            {
                if (slot.EquippedItem == item)
                    return slot;
            }

            return null;
        }

        public EquipmentSlot GetSlot(EquipmentSlotType slotType)
        {
            foreach (var slot in _slots)
            {
                if (slot.SlotType == slotType)
                    return slot;
            }

            return null;
        }

        public bool CanEquip(EquipmentSlot slot, ItemTable item)
        {
            if (slot == null || item == null)
                return false;

            if (slot.IsEquipped)
                return false;

            if (!item.ItemDataSo.IsEquippable)
                return false;

            if (GetSlotForItem(item) != null)
                return false;

            return item.ItemDataSo.EquipmentSlotType.HasFlag(slot.SlotType);
        }

        public bool TryEquip(EquipmentSlot slot, ItemTable item)
        {
            if (!CanEquip(slot, item))
                return false;

            if (item.IsRotated)
                item.Rotate();

            slot.EquippedItem = item;

            InventoryManager?.SaveEquippedItem(slot.SlotType, item);

            slot.ShowItemUI(item);

            slot.RaiseEquipped(item);
            return true;
        }

        public void Unequip(EquipmentSlot slot)
        {
            if (slot == null || slot.EquippedItem == null)
                return;

            var unequippedItem = slot.EquippedItem;

            slot.RemoveContainerSection();

            slot.EquippedItem = null;

            InventoryManager?.RemoveEquippedItem(slot.SlotType);

            slot.HideItemUI();

            slot.RaiseUnequipped(unequippedItem);
        }

        public ItemTable ExtractItem(EquipmentSlot slot, out EquipmentSlot extractedFromSlot)
        {
            extractedFromSlot = null;

            if (slot == null || slot.EquippedItem == null)
                return null;

            extractedFromSlot = slot;
            var item = slot.EquippedItem;

            slot.RemoveContainerSection();

            slot.EquippedItem = null;

            InventoryManager?.RemoveEquippedItem(slot.SlotType);

            slot.DetachItemUI();
            slot.RaiseUnequipped(item);
            return item;
        }

        public void RestoreToSlot(EquipmentSlot slot, ItemTable item, InventoryItem existingUI)
        {
            if (slot == null || item == null)
                return;

            slot.EquippedItem = item;
            slot.RestoreItemUI(item, existingUI);

            InventoryManager?.SaveEquippedItem(slot.SlotType, item);
        }

        public void RestoreSavedItem(EquipmentSlot slot)
        {
            if (slot == null || InventoryManager == null)
                return;

            var savedItem = InventoryManager.GetEquippedItem(slot.SlotType);

            if (savedItem == null || slot.IsEquipped)
                return;

            if (savedItem.IsRotated)
                savedItem.Rotate();

            slot.EquippedItem = savedItem;
            slot.ShowItemUI(savedItem);
        }

        public void SaveEquippedItem(EquipmentSlot slot)
        {
            if (slot == null || slot.EquippedItem == null)
                return;

            InventoryManager?.SaveEquippedItem(slot.SlotType, slot.EquippedItem);
        }
    }
}