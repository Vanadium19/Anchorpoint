namespace BuildingModule
{
    public interface IStorageService
    {
        bool CanBuy(string id);
        bool Buy(string id);
        BuildPriceInfo GetPriceInfo(string id);
    }
}