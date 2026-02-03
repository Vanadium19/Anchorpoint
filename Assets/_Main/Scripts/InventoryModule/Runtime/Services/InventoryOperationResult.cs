namespace InventoryModule
{
    public enum InventoryError
    {
        None,
        NoSpace,
        InvalidItem,
        PositionOccupied,
        ItemNotFound,
        CannotSplit,
        InternalError
    }

    public sealed class InventoryOperationResult
    {
        public bool Success => Error == InventoryError.None;
        public InventoryError Error { get; }
        public InventoryItem AffectedItem { get; }

        private InventoryOperationResult(InventoryError error, InventoryItem affectedItem)
        {
            Error = error;
            AffectedItem = affectedItem;
        }

        public static InventoryOperationResult CreateSuccess(InventoryItem item = null)
            => new InventoryOperationResult(InventoryError.None, item);

        public static InventoryOperationResult CreateFailure(InventoryError error, InventoryItem item = null)
            => new InventoryOperationResult(error, item);
    }
}
