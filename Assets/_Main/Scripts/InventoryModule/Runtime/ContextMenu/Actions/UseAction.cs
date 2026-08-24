using UtilsModule;
using Zenject;

namespace InventoryModule.ContextMenu.Actions
{
    public class UseAction : ContextActionBase
    {
        private const string TitleKey = "action_use";

        private readonly IItemUseHandler _useHandler;
        private readonly IEquipmentSlotService _slotService;
        private readonly object _effectTarget;

        public UseAction(
            ItemTable item,
            string displayName,
            IItemUseHandler useHandler,
            IEquipmentSlotService slotService,
            object effectTarget)
            : base(item, displayName)
        {
            _useHandler = useHandler;
            _slotService = slotService;
            _effectTarget = effectTarget;
        }

        public UseAction(
            ItemTable item,
            string displayName,
            IItemUseHandler useHandler,
            IEquipmentSlotService slotService)
            : base(item, displayName)
        {
            _useHandler = useHandler;
            _slotService = slotService;
            _effectTarget = null;
        }

        public override string DisplayName => DisplayNameOverride ?? LocalizedText.Get(TitleKey);

        public override bool IsAvailable => CanUse();

        private bool CanUse()
        {
            if (Item == null)
                return false;

            if (Item.HasDurability && Item.DurabilityMetadata?.IsDepleted == true)
                return false;

            if (_useHandler == null || _effectTarget == null)
                return false;

            var effectData = Item.ItemDataSo?.EffectData;

            if (effectData == null)
                return false;

            if (!effectData.CanApply(_effectTarget))
                return false;

            return true;
        }

        public override void Execute()
        {
            if (Item == null || _useHandler == null || _effectTarget == null)
                return;

            var effectData = Item.ItemDataSo?.EffectData;

            if (effectData == null)
                return;

            var durabilityToConsume = effectData.DurabilityPerUse;

            if (!Item.HasDurability)
            {
                _useHandler.UseItem(Item, _effectTarget);
                return;
            }

            while (Item.DurabilityMetadata.Current >= durabilityToConsume)
            {
                if (!effectData.CanApply(_effectTarget))
                    break;

                Item.DurabilityMetadata.Consume(durabilityToConsume);
                _useHandler.UseItem(Item, _effectTarget);

                if (Item.DurabilityMetadata.IsDepleted)
                {
                    RemoveIfDepleted();
                    break;
                }
            }
        }

        private void RemoveIfDepleted()
        {
            if (Item == null || !Item.HasDurability)
                return;

            if (!Item.DurabilityMetadata.IsDepleted)
                return;

            var slot = _slotService?.GetSlotForItem(Item);

            if (slot != null)
                _slotService.Unequip(slot);
            else
                Item.RemoveItselfFromLocation();
        }
    }
}
