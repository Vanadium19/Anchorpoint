using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System;

namespace InventoryModule
{
    public sealed class InventoryItemView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
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

        [Header("Split Ghost")]
        [SerializeField] private CanvasGroup splitGhost;
        [SerializeField] private Image splitGhostIcon;
        [SerializeField] private TMP_Text splitGhostAmountText;

        public event System.Action<InventoryItemView> DragStarted;
        public event System.Action<InventoryItemView, Vector2> DragEnded;
        public event System.Action<InventoryItemView> DragUpdated;
        public event Action<InventoryItemView> PointerEntered;
        public event Action<InventoryItemView> PointerExited;

        private InventoryItem _item;
        private float _tileSize;
        private float _spacing;

        private Vector2 _centeringOffset;
        private Vector2 _lastMousePosition;
        private bool _localIsRotated;
        private bool _isSplitting;
        private bool _isDragging;
        private bool _canRotate;
        private Func<bool> _splitCheck;

        public InventoryItem Item => _item;
        public bool IsSplitting => _isSplitting;
        public bool LocalIsRotated => _localIsRotated;
        public RectTransform RectTransform => rectTransform;

        private void OnEnable()
        {
            _isDragging = false;
        }

        private void OnDisable()
        {
            PointerExited?.Invoke(this);
            _isDragging = false;
        }

        public void Setup(InventoryItem item, ItemDefinition config, float tileSize, float spacing, Func<bool> splitCheck)
        {
            _item = item;
            _tileSize = tileSize;
            _spacing = spacing;
            _splitCheck = splitCheck;

            _canRotate = config.CanRotate;
            _localIsRotated = item.IsRotated;
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
            SnapToCursor(_lastMousePosition);
            DragUpdated?.Invoke(this);
        }

        public void ResetView()
        {
            _item = null;
            _isDragging = false;
            _isSplitting = false;
            _localIsRotated = false;
            _splitCheck = null;
            
            DragStarted = null;
            DragEnded = null;
            DragUpdated = null;
            PointerEntered = null;
            PointerExited = null;
            
            if (amountText != null)
            {
                amountText.text = "";
                amountText.gameObject.SetActive(false);
            }
            
            if (iconImage != null)
                iconImage.sprite = null;
                
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
            }

            if (splitGhost != null)
            {
                splitGhost.gameObject.SetActive(false);
                splitGhost.transform.SetParent(transform, false);
            }
        }

        public void RefreshAmount()
        {
            if (amountText != null && _item != null)
            {
                int showAmount = _isSplitting ? (_item.Amount / 2) : _item.Amount;
                if (showAmount > 1)
                {
                    amountText.gameObject.SetActive(true);
                    amountText.text = showAmount.ToString();
                    amountText.ForceMeshUpdate();
                }
                else
                {
                    amountText.gameObject.SetActive(false);
                }
            }
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

            RefreshAmount();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            PointerExited?.Invoke(this);
            _isDragging = true;
            _isSplitting = _splitCheck != null && _splitCheck.Invoke() && _item.Amount > 1;

            if (_isSplitting)
            {
                int moveAmount = _item.Amount / 2;
                int stayingAmount = _item.Amount - moveAmount;

                if (splitGhost != null)
                {
                    if (stayingAmount > 1 && splitGhostAmountText != null)
                    {
                        splitGhostAmountText.text = stayingAmount.ToString();
                        splitGhostAmountText.gameObject.SetActive(true);
                    }
                    else if (splitGhostAmountText != null)
                    {
                        splitGhostAmountText.gameObject.SetActive(false);
                    }

                    if (splitGhostIcon != null && iconImage != null)
                    {
                        splitGhostIcon.sprite = iconImage.sprite;
                    }

                    splitGhost.transform.SetParent(transform.parent, false);
                    splitGhost.GetComponent<RectTransform>().anchoredPosition = rectTransform.anchoredPosition;
                    splitGhost.GetComponent<RectTransform>().sizeDelta = rectTransform.sizeDelta;
                    splitGhost.alpha = splitGhostAlpha;
                    splitGhost.blocksRaycasts = false;
                    splitGhost.gameObject.SetActive(true);
                }
            }

            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = dragAlpha;
            RecalculateOffset();
            DragStarted?.Invoke(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            _lastMousePosition = eventData.position;
            SnapToCursor(_lastMousePosition);
            DragUpdated?.Invoke(this);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;

            if (splitGhost != null)
            {
                splitGhost.gameObject.SetActive(false);
                splitGhost.transform.SetParent(transform, false);
            }

            DragEnded?.Invoke(this, eventData.position);
        }

        private void RecalculateOffset()
        {
            var rect = rectTransform.rect;
            _centeringOffset = new Vector2(-rect.width * 0.5f, rect.height * 0.5f);
        }

        private void SnapToCursor(Vector2 screenPosition)
        {
            var parentRT = transform.parent as RectTransform;
            if (parentRT == null) return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRT,
                screenPosition,
                null,
                out var localMousePos))
            {
                rectTransform.anchoredPosition = localMousePos + _centeringOffset;
            }
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Cursor.lockState == CursorLockMode.Locked || _isDragging) return;

            PointerEntered?.Invoke(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            PointerExited?.Invoke(this);
        }
    }
}