using System.Collections.Generic;

namespace BaseModule
{
    public class WorkbenchCraftQueue
    {
        public List<CraftBatch> Batches { get; } = new();
        public List<ReadyCraft> ReadyItems { get; } = new();
        public int NextBatchId;
        public int NextReadyId;
    }
}
