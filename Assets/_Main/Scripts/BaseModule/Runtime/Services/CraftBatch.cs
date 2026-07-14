using UnityEngine;

namespace BaseModule
{
    public class CraftBatch
    {
        public int BatchId { get; set; }
        public RecipeConfig Recipe { get; set; }
        public int TotalCount { get; set; }
        public int CompletedCount { get; set; }
        public float CurrentCraftTime { get; set; }
        public float Progress => Recipe != null && Recipe.CraftTime > 0f
            ? Mathf.Clamp01(CurrentCraftTime / Recipe.CraftTime)
            : 0f;
        public int RemainingToCraft => TotalCount - CompletedCount;
    }
}
