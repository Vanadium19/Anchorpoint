using Zenject;

namespace InventoryModule.ContextMenu
{
    public interface IUseActionConfig
    {
        IContextAction Create(DiContainer container, ItemTable item, object effectTarget);
    }
}