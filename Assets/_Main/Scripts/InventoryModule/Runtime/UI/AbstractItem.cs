using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using InputModule;
using Zenject;

namespace InventoryModule
{
    public abstract class AbstractItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        // [Header("UI Components")]
        [SerializeField] protected Image iconImage;
        [SerializeField] protected RectTransform rectTransform;

        protected bool localIsRotated;

        protected Vector2 OriginalPosition;
        protected Transform OriginalParent;

        private Vector2 _lastMousePosition;

        private Canvas _parentCanvas;
        private CanvasGroup _canvasGroup;

        public ItemTable Item { get; private set; }

        [Inject] protected IInputMap InputMap { get; set; }
        protected bool IsDragging { get; private set; }

        protected virtual void Awake()
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();

            if (iconImage == null)
                iconImage = GetComponent<Image>();

            _canvasGroup = GetComponent<CanvasGroup>();

            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            _parentCanvas = GetComponentInParent<Canvas>();
        }

        protected virtual void OnEnable()
        {
            if (Item != null)
            {
                Item.UIUpdated += UpdateUI;
                UpdateUI();
            }
        }

        protected virtual void Update()
        {
            if (IsDragging && InputMap != null && InputMap.IsRotatePressed)
                RotateItem();

            if (IsDragging)
                HandleScrollDuringDrag();
        }

        protected virtual void OnDisable()
        {
            if (Item != null)
                Item.UIUpdated -= UpdateUI;
        }

        public void SetItem(ItemTable item)
        {
            if (Item != null)
                Item.UIUpdated -= UpdateUI;

            Item = item;

            if (item != null)
            {
                iconImage.sprite = item.ItemDataSo.Icon;
                localIsRotated = item.IsRotated;
                UpdateUI();

                if (isActiveAndEnabled)
                    item.UIUpdated += UpdateUI;
            }
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (Item == null)
                return;

            if (IsDragging)
                return;

            IsDragging = true;
            OriginalPosition = rectTransform.anchoredPosition;
            OriginalParent = transform.parent;

            var canvas = GetDragCanvas();

            if (canvas != null)
            {
                transform.SetParent(canvas.transform, true);
                transform.SetAsLastSibling();
            }

            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.alpha = 0.8f;
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (!IsDragging)
                return;

            _lastMousePosition = eventData.position;
            transform.position = (Vector3)_lastMousePosition;

            UpdateGridHighlight();
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            IsDragging = false;

            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.alpha = 1f;

            TryPlaceItem();

            HideAllHighlights();
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
        }

        protected virtual void UpdateUI()
        {
            if (Item == null)
                return;

            var tileSize = GetTileSize();
            var originalW = Item.ItemDataSo.Width;
            var originalH = Item.ItemDataSo.Height;

            var width = originalW * tileSize;
            var height = originalH * tileSize;
            rectTransform.sizeDelta = new Vector2(width, height);

            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

            var angle = localIsRotated ? -90f : 0f;
            rectTransform.rotation = Quaternion.Euler(0, 0, angle);

            if (iconImage == null || iconImage.gameObject == gameObject)
                return;

            var iconRT = iconImage.GetComponent<RectTransform>();

            if (iconRT == null)
                return;

            iconRT.anchorMin = Vector2.zero;
            iconRT.anchorMax = Vector2.one;
            iconRT.anchoredPosition = Vector2.zero;
            iconRT.sizeDelta = Vector2.zero;
            iconRT.localPosition = Vector3.zero;
            iconRT.localRotation = Quaternion.identity;
        }

        protected virtual void UpdateGridHighlight()
        {
        }

        protected virtual void HideAllHighlights()
        {
        }

        protected virtual void TryPlaceItem()
        {
        }

        private float GetTileSize()
        {
            var grid = GetComponentInParent<AbstractGrid>();
            return grid != null ? grid.TileSize : 50f;
        }

        private ScrollRect GetScrollRectUnderMouse()
        {
            Vector2 mousePosition = Input.mousePosition;
            var pointerData = new PointerEventData(EventSystem.current) { position = mousePosition, };

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (RaycastResult result in results)
            {
                if (result.gameObject == gameObject || result.gameObject.transform.IsChildOf(transform))
                    continue;

                var scrollRect = result.gameObject.GetComponent<ScrollRect>();

                if (scrollRect == null)
                    scrollRect = result.gameObject.GetComponentInParent<ScrollRect>();

                if (scrollRect != null && scrollRect.vertical)
                    return scrollRect;
            }

            return null;
        }

        private Canvas GetDragCanvas()
        {
            if (_parentCanvas != null)
                return _parentCanvas;

            _parentCanvas = GetComponentInParent<Canvas>();
            return _parentCanvas;
        }

        private void RotateItem()
        {
            if (Item is not { CanRotate: true, })
                return;

            localIsRotated = !localIsRotated;

            UpdateUI();

            transform.position = _lastMousePosition;

            UpdateGridHighlight();
        }

        private void HandleScrollDuringDrag()
        {
            var scrollDelta = Input.mouseScrollDelta.y;

            if (!(Mathf.Abs(scrollDelta) > 0.01f))
                return;

            var scrollRect = GetScrollRectUnderMouse();

            if (scrollRect == null)
                return;

            var pointerData = new PointerEventData(EventSystem.current) { scrollDelta = new(0, scrollDelta), };
            scrollRect.OnScroll(pointerData);
        }
    }
}