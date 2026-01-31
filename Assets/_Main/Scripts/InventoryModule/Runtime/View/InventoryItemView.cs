using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System;

namespace InventoryModule
{
    public class InventoryItemView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text amountText;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Settings")]
        [Range(0f, 1f)]
        [SerializeField] private float dragAlpha = 0.7f;
        [Range(0f, 1f)]
        [SerializeField] private float splitGhostAlpha = 0.5f;

        public event System.Action<InventoryItemView> DragStarted;
        public event System.Action<InventoryItemView> DragEnded;
        public event System.Action<InventoryItemView> DragUpdated;

        private InventoryItem _item;
        private float _tileSize;
        private float _spacing;

        private Vector2 _centeringOffset;
        private bool _localIsRotated;
        private bool _isSplitting;
        private int _originalAmount;
        private bool _isDragging;
        private bool _rotatePressedLastFrame;
        private bool _canRotate;
        private Func<bool> _splitCheck;

        private GameObject _splitDummy;

        public InventoryItem Item => _item;
        public bool IsSplitting => _isSplitting;
        public bool LocalIsRotated => _localIsRotated;

        public void Setup(InventoryItem item, ItemDefinition config, float tileSize, float spacing, Func<bool> splitCheck)
        {
            _item = item;
            _tileSize = tileSize;
            _spacing = spacing;
            _splitCheck = splitCheck;

            _canRotate = config.CanRotate;
            _localIsRotated = item.IsRotated;
            _originalAmount = item.Amount;
            _isDragging = false;

            if (iconImage != null)
                iconImage.sprite = config.Icon;

            UpdateVisuals();
        }

        public void Rotate()
        {
            if (!_canRotate) return;
            _localIsRotated = !_localIsRotated;
            UpdateVisuals();
            RecalculateOffset();
            SnapToCursor();
            DragUpdated?.Invoke(this);
        }

        private void UpdateVisuals()
        {
            if (_item == null) return;

            int baseW = _item.IsRotated ? _item.Size.y : _item.Size.x;
            int baseH = _item.IsRotated ? _item.Size.x : _item.Size.y;
            int currentW = _localIsRotated ? baseH : baseW;
            int currentH = _localIsRotated ? baseW : baseH;

            float widthPx = (currentW * _tileSize) + ((currentW - 1) * _spacing);
            float heightPx = (currentH * _tileSize) + ((currentH - 1) * _spacing);

            rectTransform.sizeDelta = new Vector2(widthPx, heightPx);

            if (!_isDragging)
            {
                float posX = (_item.Position.x * _tileSize) + (_item.Position.x * _spacing);
                float posY = -((_item.Position.y * _tileSize) + (_item.Position.y * _spacing));
                rectTransform.anchoredPosition = new Vector2(posX, posY);
                rectTransform.localScale = Vector3.one;
                rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, rectTransform.localPosition.y, 0);
            }

            if (amountText != null)
            {
                int showAmount = _isSplitting ? (_originalAmount / 2) : _item.Amount;
                if (showAmount > 1)
                {
                    amountText.gameObject.SetActive(true);
                    amountText.text = showAmount.ToString();
                }
                else
                {
                    amountText.gameObject.SetActive(false);
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDragging = true;
            _isSplitting = _splitCheck != null && _splitCheck.Invoke() && _item.Amount > 1;

            if (_isSplitting)
            {
                CreateSplitDummy();
                int moveAmount = _item.Amount / 2;
                if (amountText != null) amountText.text = moveAmount.ToString();
            }

            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = dragAlpha;
            RecalculateOffset();
            DragStarted?.Invoke(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            SnapToCursor();
            DragUpdated?.Invoke(this);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;

            if (_splitDummy != null)
                Destroy(_splitDummy);

            DragEnded?.Invoke(this);
        }

        private void CreateSplitDummy()
        {
            _splitDummy = Instantiate(gameObject, transform.parent);
            var clonedView = _splitDummy.GetComponent<InventoryItemView>();
            TMP_Text dummyText = clonedView != null ? clonedView.amountText : null;
            if (clonedView != null) Destroy(clonedView);

            var cg = _splitDummy.GetComponent<CanvasGroup>();
            if (cg == null) cg = _splitDummy.AddComponent<CanvasGroup>();
            cg.alpha = splitGhostAlpha;
            cg.blocksRaycasts = false;

            var dummyRect = _splitDummy.GetComponent<RectTransform>();
            if (dummyRect != null && rectTransform != null)
            {
                dummyRect.anchoredPosition = rectTransform.anchoredPosition;
                dummyRect.sizeDelta = rectTransform.sizeDelta;
            }

            int stayingAmount = _originalAmount - (_originalAmount / 2);

            if (dummyText != null)
            {
                if (stayingAmount > 1)
                {
                    dummyText.gameObject.SetActive(true);
                    dummyText.text = stayingAmount.ToString();
                }
                else
                {
                    dummyText.gameObject.SetActive(false);
                }
            }
        }

        private void RecalculateOffset()
        {
            var rect = rectTransform.rect;
            _centeringOffset = new Vector2(-rect.width * 0.5f, rect.height * 0.5f);
        }

        private void SnapToCursor()
        {
            var parentRT = transform.parent as RectTransform;
            if (parentRT == null) return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRT,
                Input.mousePosition,
                null,
                out var localMousePos))
            {
                rectTransform.anchoredPosition = localMousePos + _centeringOffset;
            }
        }
    }
}