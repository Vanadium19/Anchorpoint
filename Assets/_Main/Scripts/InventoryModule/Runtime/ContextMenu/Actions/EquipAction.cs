namespace InventoryModule.ContextMenu.Actions
{
    public class EquipAction : ContextActionBase
    {
        private readonly IEquipmentSlotService _slotService;

        public override string DisplayName => DisplayNameOverride ?? "Equip";
        public override bool IsAvailable => HasAvailableSlot();

        public EquipAction(
            ItemTable item, 
            string displayName, 
            IEquipmentSlotService slotService) 
            : base(item, displayName)
        {
            _slotService = slotService;
        }

        public override void Execute()
        {
            if (_slotService == null) return;

            foreach (var slot in _slotService.GetAllSlots())
            {
                if (slot.CanEquip(Item) && !slot.IsEquipped)
                {
                    slot.TryEquip(Item);
                    Item.RemoveItselfFromLocation();
                    return;
                }
            }
        }

        private bool HasAvailableSlot()
        {
            if (_slotService == null) return false;

            foreach (var slot in _slotService.GetAllSlots())
            {
                if (slot.CanEquip(Item))
                    return true;
            }
            return false;
        }
    }
}
