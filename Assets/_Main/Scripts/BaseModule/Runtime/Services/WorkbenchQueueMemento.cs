using System;
using System.Collections.Generic;

namespace BaseModule
{
    [Serializable]
    public class WorkbenchQueueMemento
    {
        public string InstanceId;
        public int NextBatchId;
        public int NextReadyId;
        public List<BatchMemento> Batches = new();
        public List<ReadyMemento> ReadyItems = new();
    }
}
