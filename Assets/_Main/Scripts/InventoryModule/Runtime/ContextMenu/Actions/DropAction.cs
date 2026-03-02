namespace InventoryModule.ContextMenu.Actions
{
    public class DropAction : ContextActionBase
    {
        private readonly IEquipmentSlotService _slotService;
        private readonly IDropService _dropService;

        public override string DisplayName => DisplayNameOverride ?? "Drop";
        public override bool IsAvailable => Item?.ItemDataSo?.IsDropable ?? false;

        public DropAction(
            ItemTable item,
            string displayName,
            IDropService dropService,
            IEquipmentSlotService slotService = null)
            : base(item, displayName)
        {
            _dropService = dropService;
            _slotService = slotService;
        }

        public override void Execute()
        {
            if (_dropService == null)
                return;

            if (_slotService != null)
            {
                var slot = _slotService.GetSlotForItem(Item);

                if (slot != null)
                    slot.Unequip();
            }

            if (_dropService.TryDropItem(Item))
                Item.RemoveItselfFromLocation();
        }
    }
}
