using System;

namespace InventoryModule
{
    public interface IInventoryReadyHandler
    {
        event Action InventoryReady;
    }
}
