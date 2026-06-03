using System;

namespace BaseModule
{
    [Serializable]
    public class ReadyMemento
    {
        public int ReadyId;
        public int SourceBatchId;
        public string RecipeName;
        public int Count;
    }
}
