using System.Collections.Generic;

namespace InventoryModule
{
    public interface IInventoryGridView
    {
        IReadOnlyList<GridTable> Grids { get; }
    }
}