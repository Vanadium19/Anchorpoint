using System;

namespace BaseModule
{
    [Serializable]
    public class BatchMemento
    {
        public int BatchId;
        public string RecipeName;
        public int TotalCount;
        public int CompletedCount;
        public double CurrentCraftTime;
    }
}
