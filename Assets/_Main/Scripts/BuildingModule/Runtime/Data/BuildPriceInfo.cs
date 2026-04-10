using System.Collections.Generic;

namespace BuildingModule
{
    public class BuildPriceInfo
    {
        public string BuildingName { get; set; }
        public int AvailableCount { get; set; }
        public List<PriceItemInfo> Items { get; set; } = new();
    }
}
