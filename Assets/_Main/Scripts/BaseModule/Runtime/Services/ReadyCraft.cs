namespace BaseModule
{
    public class ReadyCraft
    {
        public int ReadyId { get; set; }
        public int SourceBatchId { get; set; }
        public RecipeConfig Recipe { get; set; }
        public int Count { get; set; } = 1;
    }
}
