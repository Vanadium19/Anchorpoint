using System;
using System.Collections.Generic;
using BaseModule;

namespace CampSaveModule
{
    [Serializable]
    public class WorkbenchQueuesMemento
    {
        public long SavedAtUnixMs;
        public List<WorkbenchQueueMemento> Queues = new();
    }
}
