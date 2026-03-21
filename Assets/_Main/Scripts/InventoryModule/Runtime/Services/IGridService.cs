using System.Collections.Generic;

namespace InventoryModule
{
    public interface IGridService
    {
        void RegisterGrid(AbstractGrid grid);
        void UnregisterGrid(AbstractGrid grid);
        IReadOnlyList<AbstractGrid> GetAllGrids();
    }
}
