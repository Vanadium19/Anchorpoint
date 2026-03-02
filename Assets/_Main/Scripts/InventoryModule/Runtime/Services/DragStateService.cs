namespace InventoryModule
{
    public sealed class DragStateService : IDragStateService
    {
        public AbstractItem CurrentlyDraggedItem { get; set; }

        public bool IsDragging => CurrentlyDraggedItem != null;
    }
}
