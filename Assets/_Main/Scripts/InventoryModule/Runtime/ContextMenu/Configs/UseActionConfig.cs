using Zenject;

namespace InventoryModule.ContextMenu.Configs
{
    [System.Serializable]
    public class UseActionConfig : ActionConfigBase, IUseActionConfig
    {
        public override string ActionType => "Use";

        public override IContextAction Create(DiContainer container, ItemTable item)
        {
            var useHandler = container.TryResolve<IItemUseHandler>();
            var slotService = container.TryResolve<IEquipmentSlotService>();
            return new Actions.UseAction(item, GetDisplayName(), useHandler, slotService);
        }

        public IContextAction Create(DiContainer container, ItemTable item, object effectTarget)
        {
            var useHandler = container.TryResolve<IItemUseHandler>();
            var slotService = container.TryResolve<IEquipmentSlotService>();
            return new Actions.UseAction(item, GetDisplayName(), useHandler, slotService, effectTarget);
        }
    }
}
