namespace ComponentsModule
{
    public interface IRaidPreparationProvider
    {
        bool TryGetPreparation(out RaidPreparationInfo preparationInfo);
    }
}
