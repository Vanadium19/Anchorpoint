using System;
using System.Collections.Generic;

namespace BuildingModule
{
    [Serializable]
    public class BuildingContainersMemento
    {
        public List<BuildingContainerMemento> Buildings = new();
    }
}
