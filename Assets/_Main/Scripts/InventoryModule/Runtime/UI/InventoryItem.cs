using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using Zenject;
using InventoryModule.ContextMenu;
using InventoryModule.ContextMenu.UI;

namespace InventoryModule
{
    public class InventoryItem : AbstractItem
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI stackText;
        [SerializeField] private ContainerWindow containerWindowPrefab;
        [SerializeField] private ContextMenuPresenter contextMenuPrefab;

        private Color _originalItemColor = Color.white;

        private IContextActionService _contextActionService;
        private IContainerWindowService _windowService;
        private IContextMenuStateService _menuStateService;
        private DiContainer _diContainer;
        private DragPlacementController _placementController;
        private ContextMenuPresenter _contextMenu;

        private Vector2 _stackTextOriginalPos;
        private bool _stackTextPosInitialized;

        [Inject]
        private void Construct(DragPlacementController placementController,
            IContextActionService contextActionService,
            IContainerWindowService windowService,
            IContextMenuStateService menuStateService,
            DiContainer container)
        {
            _placementController = placementController;
            _contextActionService = contextActionService;
            _windowService = windowService;
            _menuStateService = menuStateService;
            _diContainer = container;
        }

        protected override void Awake()
        {
            base.Awake();

            if (stackText == null || _stackTextPosInitialized)
                return;

            _stackTextOriginalPos = stackText.rectTransform.anchoredPosition;
            _stackTextPosInitialized = true;
        }

        private void OnDestroy()
        {
            UnsubscribeFromDurability();

            if (_contextMenu == null)
                return;

            Destroy(_contextMenu.gameObject);
            _contextMenu = null;
        }

        public override void SetItem(ItemTable item)
        {
            UnsubscribeFromDurability();
            base.SetItem(item);
            SubscribeToDurability();
        }

        private void SubscribeToDurability()
        {
            if (Item?.DurabilityMetadata != null)
                Item.DurabilityMetadata.DurabilityChanged += OnDurabilityChanged;
        }

        private void UnsubscribeFromDurability()
        {
            if (Item?.DurabilityMetadata != null)
                Item.DurabilityMetadata.DurabilityChanged -= OnDurabilityChanged;
        }

        private void OnDurabilityChanged(int current, int max)
        {
            UpdateStackAndDurabilityDisplay();
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);

            if (Item == null)
                return;

            if (eventData.button == PointerEventData.InputButton.Right)
                ShowContextMenu(eventData.position);
            else if (eventData.clickCount == 2 && Item.IsContainer)
                OpenContainerWindow();
        }

        protected override void UpdateUI()
        {
            base.UpdateUI();

            if (stackText == null || Item == null)
                return;

            UpdateStackAndDurabilityDisplay();
        }

        protected override void UpdateGridHighlight()
        {
            if (_placementController != null)
                _placementController.UpdatePreview(this);
        }

        protected override void HideAllHighlights()
        {
            if (_placementController != null)
                _placementController.HideAllHighlights();
        }

        protected override void TryPlaceItem()
        {
            if (_placementController == null || Item == null)
                return;

            _placementController.ResolvePlacement(this);

            if (_placementController.ShouldDestroyView)
                Destroy(gameObject);
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (IsDragging)
                return;

            if (_contextMenu != null)
            {
                Destroy(_contextMenu.gameObject);
                _contextMenu = null;
            }

            base.OnBeginDrag(eventData);

            if (_placementController != null)
                _placementController.BeginDrag(this, base.OriginalParent);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (!IsDragging)
                return;

            base.OnDrag(eventData);

            if (_placementController != null)
                _placementController.UpdatePreview(this);
        }

        public void SetContainerHighlight(bool highlighted)
        {
            if (iconImage == null)
                return;

            if (highlighted)
            {
                _originalItemColor = iconImage.color;
                var highlightColor = _originalItemColor;
                highlightColor.a = 0.7f;
                highlightColor.g = Mathf.Min(1f, highlightColor.g + 0.3f);
                iconImage.color = highlightColor;
            }
            else
            {
                iconImage.color = _originalItemColor;
            }
        }

        public void SetStackHighlight(bool highlighted)
        {
            if (iconImage == null)
                return;

            if (highlighted)
            {
                _originalItemColor = iconImage.color;
                iconImage.color = new Color(0.5f, 1f, 0.5f, 1f);
            }
            else
            {
                iconImage.color = _originalItemColor;
            }
        }

        public Transform DragParent => base.OriginalParent;

        public Vector2 DragOriginPosition => base.OriginalPosition;

        public bool IsLocalRotated => base.localIsRotated;

        public void RefreshAfterRestore()
        {
            base.localIsRotated = Item.IsRotated;
            UpdateUI();
        }

        public void ReparentTo(Transform parent)
        {
            if (parent == null)
                return;

            transform.SetParent(parent, false);

            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }

        public bool UpdatePositionInGrid()
        {
            var grid = base.OriginalParent?.GetComponent<AbstractGrid>();

            if (grid == null)
                return false;

            grid.UpdateItemPosition(this);
            return true;
        }

        public void SetAnchoredTo(Vector2 position)
        {
            rectTransform.anchoredPosition = position;
        }

        public void ReturnToSlot(EquipmentSlot slot)
        {
            if (slot == null)
                return;

            transform.SetParent(slot.transform, false);

            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
        }

        private void UpdateStackAndDurabilityDisplay()
        {
            if (stackText == null)
                return;

            if (Item.HasDurability && Item.DurabilityMetadata != null)
            {
                var showDurability = Item.ItemDataSo?.ShowDurability ?? true;
                stackText.gameObject.SetActive(showDurability);

                if (showDurability)
                {
                    var metadata = Item.DurabilityMetadata;
                    stackText.text = $"{metadata.Current}/{metadata.Max}";
                }
            }
            else if (Item.IsStackable)
            {
                stackText.text = Item.StackCount.ToString();
                stackText.gameObject.SetActive(true);
            }
            else
            {
                stackText.gameObject.SetActive(false);
            }

            UpdateStackTextTransform();
        }

        private void ShowContextMenu(Vector2 screenPosition)
        {
            if (Item == null)
                return;

            if (contextMenuPrefab == null)
                return;

            var contextService = _contextActionService;

            if (contextService == null || !contextService.IsInitialized)
                return;

            if (_contextMenu == null)
            {
                Canvas canvas = GetComponentInParent<Canvas>();

                if (canvas == null)
                    return;

                _contextMenu = Instantiate(contextMenuPrefab, canvas.transform);
            }

            _contextMenu.Initialize(contextService, _menuStateService);
            _contextMenu.ShowForItem(Item, screenPosition);
        }

        private void OpenContainerWindow()
        {
            var metadata = Item.GetMetadata<ContainerMetadata>();

            if (metadata == null || containerWindowPrefab == null)
                return;

            if (_windowService.IsContainerOpen(Item))
                return;

            var canvas = GetComponentInParent<Canvas>();

            if (canvas == null)
                return;

            var window = _diContainer != null
                ? _diContainer.InstantiatePrefabForComponent<ContainerWindow>(containerWindowPrefab, canvas.transform)
                : Instantiate(containerWindowPrefab, canvas.transform);
            window.transform.SetAsLastSibling();
            window.Initialize(Item, metadata, containerWindowPrefab.GridPrefab, _windowService);
        }

        private void UpdateStackTextTransform()
        {
            if (stackText == null)
                return;

            var textRT = stackText.rectTransform;

            if (base.localIsRotated)
            {
                float offset = 50f * Item.ItemDataSo.Height;
                textRT.localRotation = Quaternion.Euler(0, 0, 90f);
                textRT.anchoredPosition = _stackTextOriginalPos + new Vector2(0, offset);
            }
            else
            {
                textRT.localRotation = Quaternion.identity;
                textRT.anchoredPosition = _stackTextOriginalPos;
            }
        }
    }
}