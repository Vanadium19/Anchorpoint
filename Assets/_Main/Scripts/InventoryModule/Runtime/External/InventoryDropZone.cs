using UnityEngine;

namespace InventoryModule
{
    public class InventoryDropZone : MonoBehaviour
    {
        [Header("Drop Settings")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private float dropDistance = 2f;
        [SerializeField] private float dropOffsetY = 0.5f;

        [Header("UI")]
        [SerializeField] private UnityEngine.UI.Image highlightImage;
        [SerializeField] private Color normalColor = new Color(1, 1, 1, 0.3f);
        [SerializeField] private Color hoverColor = new Color(1, 0.5f, 0.5f, 0.5f);

        public bool TryDropItem(ItemTable item)
        {
            if (item == null) return false;

            if (!item.ItemDataSo.IsDropable) return false;

            LootItemView prefab = item.ItemDataSo.WorldPrefab;
            if (prefab == null) return false;

            if (item.IsContainer)
            {
                ContainerWindow.CloseAllWindowsForItem(item);
            }

            Vector3 dropPosition = GetDropPosition();

            LootItemView lootInstance = Instantiate(prefab, dropPosition, Quaternion.identity);
            lootInstance.SetItemTable(item);

            return true;
        }

        private Vector3 GetDropPosition()
        {
            if (playerTransform == null)
            {
                playerTransform = Camera.main?.transform;
            }

            if (playerTransform == null)
            {
                return Vector3.zero;
            }

            Vector3 forward = playerTransform.forward;
            forward.y = 0;
            forward.Normalize();

            Vector3 dropPos = playerTransform.position + forward * dropDistance;
            dropPos.y += dropOffsetY;

            return dropPos;
        }

        public void ShowHighlight(bool show)
        {
            if (highlightImage != null)
            {
                highlightImage.color = show ? hoverColor : normalColor;
            }
        }
    }
}
