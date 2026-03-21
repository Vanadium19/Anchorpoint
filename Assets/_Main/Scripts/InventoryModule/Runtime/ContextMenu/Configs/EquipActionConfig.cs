using Zenject;

namespace InventoryModule.ContextMenu.Configs
{
    [System.Serializable]
    public class EquipActionConfig : ActionConfigBase
    {
        public override string ActionType => "Equip";

        public override IContextAction Create(DiContainer container, ItemTable item)
        {
            var slotService = container.TryResolve<IEquipmentSlotService>();
            return new Actions.EquipAction(item, GetDisplayName(), slotService);
        }
    }
}