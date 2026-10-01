namespace BuildingModule
{
    public interface IStorageService
    {
        bool CanBuy(string id);
        bool Buy(string id);
        bool CanAfford(Price price);
        bool CanAfford(Price price, float costMultiplier);
        bool Spend(Price price);
        bool Spend(Price price, float costMultiplier);
        BuildPriceInfo GetPriceInfo(string id);
        BuildPriceInfo GetPriceInfo(string displayName, Price price);
        BuildPriceInfo GetPriceInfo(string displayName, Price price, float costMultiplier);
    }
}
