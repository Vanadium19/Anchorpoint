using System.Collections.Generic;

namespace InventoryModule
{
    public interface IInventoryContainer
    {
        string DisplayName { get; }
        IReadOnlyList<GridTable> Grids { get; }
    }
}
