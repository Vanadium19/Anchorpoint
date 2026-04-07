using System;
using System.Collections.Generic;
using InventoryModule;

namespace BuildingModule
{
    [Serializable]
    public class BuildingContainersMemento
    {
        public List<BuildingContainerMemento> Buildings = new();
    }

    [Serializable]
    public class BuildingContainerMemento
    {
        public int BuildingInstanceId;
        public List<ContainerMemento> Containers = new();
    }
}
