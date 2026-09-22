using System;
using System.Collections.Generic;

namespace InventoryModule
{
    public interface IInventoryGridView
    {
        event Action GridsChanged;

        IReadOnlyList<GridTable> Grids { get; }
    }
}
