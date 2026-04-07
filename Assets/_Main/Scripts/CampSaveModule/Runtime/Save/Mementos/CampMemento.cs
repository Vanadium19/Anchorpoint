using System;
using System.Collections.Generic;
using BuildingModule;

namespace CampSaveModule
{
    [Serializable]
    public class CampMemento
    {
        public int BasePoints;
        public List<BuildingSnapshot> Buildings = new();
    }
}
