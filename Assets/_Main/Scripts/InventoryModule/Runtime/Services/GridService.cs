using System.Collections.Generic;

namespace InventoryModule
{
    public sealed class GridService : IGridService
    {
        private readonly List<AbstractGrid> _grids = new List<AbstractGrid>();

        public void RegisterGrid(AbstractGrid grid)
        {
            if (grid != null && !_grids.Contains(grid))
                _grids.Add(grid);
        }

        public void UnregisterGrid(AbstractGrid grid)
        {
            if (grid != null)
                _grids.Remove(grid);
        }

        public IReadOnlyList<AbstractGrid> GetAllGrids()
        {
            return _grids;
        }
    }
}
