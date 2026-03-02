using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace InventoryModule
{
    public class ItemDragGhost : MonoBehaviour, IItemDragGhostService
    {
        [SerializeField] private Image ghostImage;
        [SerializeField] private RectTransform rectTransform;

        private Canvas _parentCanvas;

        public void Initialize(Canvas canvas)
        {
            _parentCanvas = canvas;
            
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();

            if (ghostImage == null)
                ghostImage = GetComponent<Image>();

            var canvasGroup = GetComponent<CanvasGroup>();

            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 0.5f;
            canvasGroup.blocksRaycasts = false;

            gameObject.SetActive(false);
        }

        public void Show(ItemTable item, Vector2 position, Vector2 size)
        {
            gameObject.SetActive(true);

            if (item != null && item.ItemDataSo != null && item.ItemDataSo.Icon != null)
            {
                ghostImage.sprite = item.ItemDataSo.Icon;
                ghostImage.color = new Color(1, 1, 1, 0.5f);
            }

            rectTransform.position = position;
            rectTransform.sizeDelta = size;

            rectTransform.rotation = Quaternion.Euler(0, 0, item.IsRotated ? -90f : 0f);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void SetValid(bool isValid)
        {
            if (isValid)
                ghostImage.color = new Color(0.5f, 1f, 0.5f, 0.5f);
            else
                ghostImage.color = new Color(1f, 0.5f, 0.5f, 0.5f);
        }

        public class Factory : PlaceholderFactory<ItemDragGhost>
        {
        }
    }
}
