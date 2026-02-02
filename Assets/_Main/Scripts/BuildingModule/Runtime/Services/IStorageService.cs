namespace BuildingModule
{
    public interface IStorageService
    {
        bool CanBuy(BuildingName name);
        bool Buy(BuildingName name);
    }
}