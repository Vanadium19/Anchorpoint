using System;
using System.Collections.Generic;

namespace SaveModule
{
    [Serializable]
    public class GameSaveData
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public Dictionary<string, string> State = new();
    }
}
