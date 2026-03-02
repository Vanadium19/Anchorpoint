using UnityEngine;
using Zenject;

namespace InventoryModule.ContextMenu.Configs
{
    [System.Serializable]
    public class DropActionConfig : ActionConfigBase
    {
        public override string ActionType => "Drop";

    public override IContextAction Create(DiContainer container, ItemTable item)
    {
        var dropService = container.TryResolve<IDropService>();
        var slotService = container.TryResolve<IEquipmentSlotService>();
        return new Actions.DropAction(item, GetDisplayName(), dropService, slotService);
    }
    }
}
