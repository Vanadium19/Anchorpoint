namespace InventoryModule
{
    public enum PlacementOutcome
    {
        None,
        PlacedToGrid,
        StackedFully,
        StackedPartially,
        Equipped,
        InsertedIntoContainer,
        DroppedToWorld,
        Returned,
    }
}
