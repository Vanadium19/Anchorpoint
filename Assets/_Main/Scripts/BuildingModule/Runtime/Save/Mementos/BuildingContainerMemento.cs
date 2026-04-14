using System;
using System.Collections.Generic;
using InventoryModule;

namespace BuildingModule
{
    [Serializable]
    public class BuildingContainerMemento
    {
        public int BuildingInstanceId;
        public List<ContainerMemento> Containers = new();
    }
}
