namespace InventoryModule
{
    public interface IDragStateService
    {
        AbstractItem CurrentlyDraggedItem { get; set; }
        bool IsDragging { get; }
    }
}
