using System;
using System.Collections.Generic;

namespace SaveModule
{
    [Serializable]
    public class GameSaveData
    {
        public Dictionary<string, string> State = new();
    }
}
