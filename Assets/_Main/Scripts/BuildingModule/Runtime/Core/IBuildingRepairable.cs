namespace BuildingModule
{
    public interface IBuildingRepairable
    {
        bool CanRepair { get; }

        void Repair();
    }
}
