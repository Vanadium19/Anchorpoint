using System;
using System.Collections.Generic;
using InventoryModule;

namespace BuildingModule
{
    [Serializable]
    public class BuildingSnapshot
    {
        public string BuildingId;
        public float PositionX;
        public float PositionY;
        public float PositionZ;
        public float RotationY;
        public List<ContainerMemento> Containers = new();
    }
}
