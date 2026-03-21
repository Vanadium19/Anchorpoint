namespace InventoryModule.ContextMenu.Actions
{
    public class DropAction : ContextActionBase
    {
        private readonly IEquipmentSlotService _slotService;
        private readonly IDropService _dropService;

        public DropAction(ItemTable item,
            string displayName,
            IDropService dropService,
            IEquipmentSlotService slotService = null)
            : base(item, displayName)
        {
            _dropService = dropService;
            _slotService = slotService;
        }

        public override string DisplayName => DisplayNameOverride ?? "Drop";
        public override bool IsAvailable => Item?.ItemDataSo?.IsDropable ?? false;

        public override void Execute()
        {
            if (_dropService == null)
                return;

            if (!_dropService.TryDropItem(Item))
                return;

            Item.RemoveItselfFromLocation();

            if (_slotService != null)
            {
                var slot = _slotService.GetSlotForItem(Item);
                slot?.Unequip();
            }
        }
    }
}