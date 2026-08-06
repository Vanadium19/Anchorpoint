namespace InventoryModule.ContextMenu.Actions
{
    public class EquipAction : ContextActionBase
    {
        private readonly IEquipmentSlotService _slotService;

        public EquipAction(ItemTable item,
            string displayName,
            IEquipmentSlotService slotService)
            : base(item, displayName)
        {
            _slotService = slotService;
        }

        public override string DisplayName => DisplayNameOverride ?? "Equip";
        public override bool IsAvailable => HasAvailableSlot();

        public override void Execute()
        {
            if (_slotService == null)
                return;

            foreach (var slot in _slotService.GetAllSlots())
            {
                if (_slotService.CanEquip(slot, Item) && !slot.IsEquipped)
                {
                    _slotService.TryEquip(slot, Item);
                    Item.RemoveItselfFromLocation();
                    return;
                }
            }
        }

        private bool HasAvailableSlot()
        {
            if (_slotService == null)
                return false;

            foreach (var slot in _slotService.GetAllSlots())
            {
                if (_slotService.CanEquip(slot, Item))
                    return true;
            }

            return false;
        }
    }
}