namespace BuildingModule
{
    public interface IStorageService
    {
        bool CanBuy(string id);
        bool Buy(string id);
        bool CanAfford(Price price);
        bool Spend(Price price);
        BuildPriceInfo GetPriceInfo(string id);
        BuildPriceInfo GetPriceInfo(string displayName, Price price);
    }
}
