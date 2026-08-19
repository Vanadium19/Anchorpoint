using System.Collections.Generic;

namespace InventoryModule
{
    public sealed class ContainerGridBuildResult
    {
        public List<AbstractGrid> Grids { get; } = new();

        public GridTable PrimaryGrid { get; set; }
    }
}
