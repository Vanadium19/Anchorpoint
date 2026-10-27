using UnityEngine;

namespace InventoryModule
{
    public class LootPickupService
    {
        private const EquipmentSlotType WeaponSlots =
            EquipmentSlotType.PrimaryWeapon1 | EquipmentSlotType.PrimaryWeapon2 | EquipmentSlotType.WeaponSecondary;

        private readonly IInventoryManager _inventoryManager;
        private readonly IDeathLootStorage _deathLootStorage;
        private readonly IEquipmentSlotService _slotService;
        private readonly IDropService _dropService;

        public LootPickupService(
            IInventoryManager inventoryManager,
            IDeathLootStorage deathLootStorage,
            IEquipmentSlotService slotService,
            IDropService dropService)
        {
            _inventoryManager = inventoryManager;
            _deathLootStorage = deathLootStorage;
            _slotService = slotService;
            _dropService = dropService;
        }

        public bool Collect(LootItemView loot)
        {
            if (loot == null || loot.ItemData == null)
                return false;

            var success = loot.ItemData.IsEquippable
                ? CollectEquippable(loot)
                : CollectSimple(loot);

            if (success && loot.ItemTable != null)
                _deathLootStorage.RemoveItem(loot.ItemTable);

            return success;
        }

        private bool CollectEquippable(LootItemView loot)
        {
            var item = loot.ItemTable ?? new ItemTable(loot.ItemData) { StackCount = loot.Amount };

            if (_inventoryManager.TryAutoEquipItem(item))
                return true;

            if (TryReplaceEquippedWeapon(item, loot.transform.position))
                return true;

            return loot.ItemTable != null
                ? _inventoryManager.AddExistingItemToInventory(loot.ItemTable)
                : _inventoryManager.AddItemToInventory(loot.ItemData, loot.Amount);
        }

        private bool CollectSimple(LootItemView loot)
        {
            return loot.ItemTable != null
                ? _inventoryManager.AddExistingItemToInventory(loot.ItemTable)
                : _inventoryManager.AddItemToInventory(loot.ItemData, loot.Amount);
        }

        private bool TryReplaceEquippedWeapon(ItemTable item, Vector3 dropPosition)
        {
            var slotType = item.ItemDataSo.EquipmentSlotType;

            if (slotType == EquipmentSlotType.None || (slotType & ~WeaponSlots) != 0)
                return false;

            var slot = FindReplaceableSlot(item);

            if (slot == null)
                return false;

            if (!_dropService.TryDropItem(slot.EquippedItem, dropPosition))
                return false;

            _slotService.Unequip(slot);
            return _slotService.TryEquip(slot, item);
        }

        private EquipmentSlot FindReplaceableSlot(ItemTable item)
        {
            var activeSlot = _slotService.ActiveSlot;

            if (IsReplaceable(activeSlot, item))
                return activeSlot;

            foreach (var slot in _slotService.GetAllSlots())
            {
                if (IsReplaceable(slot, item))
                    return slot;
            }

            return null;
        }

        private static bool IsReplaceable(EquipmentSlot slot, ItemTable item) =>
            slot != null && slot.IsEquipped && item.ItemDataSo.EquipmentSlotType.HasFlag(slot.SlotType);
    }
}
