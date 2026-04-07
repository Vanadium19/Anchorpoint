namespace SaveModule
{
    public interface ISaveable
    {
        string SaveKey { get; }
        string CreateMementoJson();
        void RestoreMementoFromJson(string json);
    }
}
