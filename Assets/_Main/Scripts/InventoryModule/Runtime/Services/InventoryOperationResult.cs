namespace InventoryModule
{
    public class InventoryOperationResult
    {
        public bool Success { get; }
        public string ErrorMessage { get; }
        public InventoryItem AffectedItem { get; }

        private InventoryOperationResult(bool success, string errorMessage, InventoryItem affectedItem)
        {
            Success = success;
            ErrorMessage = errorMessage;
            AffectedItem = affectedItem;
        }

        public static InventoryOperationResult CreateSuccess(InventoryItem item = null)
            => new InventoryOperationResult(true, string.Empty, item);

        public static InventoryOperationResult CreateFailure(string errorMessage, InventoryItem item = null)
            => new InventoryOperationResult(false, errorMessage, item);
    }
}