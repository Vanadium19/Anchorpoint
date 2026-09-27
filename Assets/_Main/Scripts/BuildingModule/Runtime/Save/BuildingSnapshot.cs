using System;
using System.Collections.Generic;
using InventoryModule;

namespace BuildingModule
{
    [Serializable]
    public class BuildingSnapshot
    {
        public string BuildingId;
        public string InstanceId;
        public float PositionX;
        public float PositionY;
        public float PositionZ;
        public float RotationY;
        public bool HasRuntimeState;
        public BuildingState State;
        public float CurrentHealth;
        public float ConstructionRemainingTime;
        public List<ContainerMemento> Containers = new();
    }
}
