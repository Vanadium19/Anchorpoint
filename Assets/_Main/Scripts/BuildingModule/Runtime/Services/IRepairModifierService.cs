namespace BuildingModule
{
    public interface IRepairModifierService
    {
        float CostMultiplier { get; }
        float RepairReductionPercent { get; }

        float GetRepairDuration(float baseDuration);
    }
}
