using UnityEngine;

namespace InventoryModule
{
    public interface IItemDragGhostService
    {
        void Show(ItemTable item, Vector2 position, Vector2 size);
        void Hide();
        void SetValid(bool isValid);
    }
}
