using System.Collections.Generic;

namespace InventoryModule
{
    public sealed class ContainerWindowService : IContainerWindowService
    {
        private readonly HashSet<ItemTable> _openContainers = new HashSet<ItemTable>();
        private readonly List<ContainerWindow> _openWindows = new List<ContainerWindow>();

        public bool IsContainerOpen(ItemTable item)
        {
            return _openContainers.Contains(item);
        }

        public void CloseAllWindowsForItem(ItemTable item)
        {
            if (item == null)
                return;

            for (int i = _openWindows.Count - 1; i >= 0; i--)
            {
                var window = _openWindows[i];

                if (window != null && window.ContainerItem == item)
                    window.Close();
            }

            if (item.IsContainer)
            {
                var metadata = item.GetMetadata<ContainerMetadata>();

                if (metadata?.Inventories != null)
                {
                    foreach (var grid in metadata.Inventories)
                    {
                        var items = grid.GetAllItems();

                        foreach (var nestedItem in items)
                        {
                            if (nestedItem.IsContainer)
                                CloseAllWindowsForItem(nestedItem);
                        }
                    }
                }
            }
        }

        public void RegisterWindow(ItemTable item, ContainerWindow window)
        {
            if (item != null)
                _openContainers.Add(item);

            if (window != null && !_openWindows.Contains(window))
                _openWindows.Add(window);
        }

        public void UnregisterWindow(ItemTable item, ContainerWindow window)
        {
            if (item != null)
                _openContainers.Remove(item);

            if (window != null)
                _openWindows.Remove(window);
        }

        public void Clear()
        {
            _openContainers.Clear();
            _openWindows.Clear();
        }
    }
}
