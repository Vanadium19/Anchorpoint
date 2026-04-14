using Zenject;

namespace InventoryModule.ContextMenu.Configs
{
    [System.Serializable]
    public class OpenActionConfig : ActionConfigBase
    {
        public override string ActionType => "Open";

        public override IContextAction Create(DiContainer container, ItemTable item)
        {
            var contextService = container.TryResolve<IContextActionService>();
            var windowService = container.TryResolve<IContainerWindowService>();

            if (contextService == null || windowService == null)
                return null;

            return new Actions.OpenAction(item,
                GetDisplayName(),
                contextService.ContainerWindowPrefab,
                contextService.GridPrefab,
                contextService.Canvas,
                windowService,
                container);
        }
    }
}