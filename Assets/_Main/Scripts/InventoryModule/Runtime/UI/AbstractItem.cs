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
        [Header("UI Components")]
        [SerializeField] protected Image iconImage;
        [SerializeField] protected RectTransform rectTransform;

        [Inject]
        protected IInputMap InputMap { get; set; }

        public ItemTable Item { get; protected set; }
        public bool IsDragging { get; protected set; }

        protected bool _localIsRotated;

        protected Vector2 originalPosition;
        protected Transform originalParent;
        protected static AbstractItem currentlyDraggedItem;
        protected Vector2 _lastMousePosition;

        private Canvas _parentCanvas;
        private CanvasGroup _canvasGroup;

        protected virtual void Awake()
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();
            if (iconImage == null)
                iconImage = GetComponent<Image>();

            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            _parentCanvas = GetComponentInParent<Canvas>();
        }

        public virtual void SetItem(ItemTable item)
        {
            Item = item;
            if (item != null)
            {
                iconImage.sprite = item.ItemDataSo.Icon;
                item.UIUpdated += UpdateUI;
                _localIsRotated = item.IsRotated;
                UpdateUI();
            }
        }

        protected virtual float GetTileSize()
        {
            var grid = GetComponentInParent<AbstractGrid>();
            return grid != null ? grid.TileSize : 50f;
        }

        protected virtual void UpdateUI()
        {
            if (Item == null) return;

            float tileSize = GetTileSize();
            int originalW = Item.ItemDataSo.Width;
            int originalH = Item.ItemDataSo.Height;

            float width = originalW * tileSize;
            float height = originalH * tileSize;
            rectTransform.sizeDelta = new Vector2(width, height);

            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

            float angle = _localIsRotated ? -90f : 0f;
            rectTransform.rotation = Quaternion.Euler(0, 0, angle);

            if (iconImage != null && iconImage.gameObject != gameObject)
            {
                var iconRT = iconImage.GetComponent<RectTransform>();
                if (iconRT != null)
                {
                    iconRT.anchorMin = Vector2.zero;
                    iconRT.anchorMax = Vector2.one;
                    iconRT.anchoredPosition = Vector2.zero;
                    iconRT.sizeDelta = Vector2.zero;
                    iconRT.localPosition = Vector3.zero;
                    iconRT.localRotation = Quaternion.identity;
                }
            }
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (Item == null) return;

            if (IsDragging) return;

            IsDragging = true;
            currentlyDraggedItem = this;
            originalPosition = rectTransform.anchoredPosition;
            originalParent = transform.parent;

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
            if (!IsDragging) return;

            _lastMousePosition = eventData.position;
            transform.position = (Vector3)_lastMousePosition;

            UpdateGridHighlight();
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            IsDragging = false;
            currentlyDraggedItem = null;

            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.alpha = 1f;

            TryPlaceItem();

            HideAllHighlights();
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
        }

        protected virtual void Update()
        {
            if (IsDragging && InputMap != null && InputMap.IsRotatePressed)
            {
                RotateItem();
            }

            if (IsDragging)
            {
                HandleScrollDuringDrag();
            }
        }

        protected virtual void HandleScrollDuringDrag()
        {
            float scrollDelta = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scrollDelta) > 0.01f)
            {
                ScrollRect scrollRect = GetScrollRectUnderMouse();
                if (scrollRect != null)
                {
                    PointerEventData pointerData = new PointerEventData(EventSystem.current);
                    pointerData.scrollDelta = new Vector2(0, scrollDelta * InventoryPanel.ScrollMultiplier);
                    scrollRect.OnScroll(pointerData);
                }
            }
        }

        protected ScrollRect GetScrollRectUnderMouse()
        {
            Vector2 mousePos = Input.mousePosition;
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = mousePos
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (RaycastResult result in results)
            {
                if (result.gameObject == gameObject || result.gameObject.transform.IsChildOf(transform))
                    continue;

                var scrollRect = result.gameObject.GetComponent<ScrollRect>();
                if (scrollRect == null)
                {
                    scrollRect = result.gameObject.GetComponentInParent<ScrollRect>();
                }

                if (scrollRect != null && scrollRect.vertical)
                {
                    return scrollRect;
                }
            }

            return null;
        }

        protected virtual void RotateItem()
        {
            if (Item != null && Item.CanRotate)
            {
                _localIsRotated = !_localIsRotated;

                UpdateUI();

                transform.position = (Vector3)_lastMousePosition;

                UpdateGridHighlight();
            }
        }

        public bool LocalIsRotated => _localIsRotated;

        protected virtual void UpdateGridHighlight()
        {
        }

        protected virtual void HideAllHighlights()
        {
        }

        protected virtual void TryPlaceItem()
        {
        }

        protected Canvas GetDragCanvas()
        {
            if (_parentCanvas != null)
                return _parentCanvas;

            _parentCanvas = GetComponentInParent<Canvas>();
            return _parentCanvas;
        }

        protected virtual void OnDestroy()
        {
            if (Item != null)
                Item.UIUpdated -= UpdateUI;
        }
    }
}
