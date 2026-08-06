using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using Zenject;

namespace InventoryModule
{
    public class ContainerSection : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform headerRect;
        [SerializeField] private RectTransform contentContainer;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Button toggleButton;
        [SerializeField] private Image expandIcon;

        [Header("Settings")]
        [SerializeField] private Sprite expandedIcon;
        [SerializeField] private Sprite collapsedIcon;
        [SerializeField] private float collapsedHeight = 30f;

        private ContainerSectionPresenter _presenter;
        private bool _isExpanded = true;

        private RectTransform _rectTransform;
        private LayoutElement _layoutElement;

        public ItemTable ContainerItem => _presenter?.ContainerItem;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _layoutElement = GetComponent<LayoutElement>();

            if (toggleButton != null)
                toggleButton.onClick.AddListener(OnToggleClicked);
        }

        private void LateUpdate()
        {
            if (_presenter == null || !gameObject.activeInHierarchy)
                return;

            _presenter.TickLateRefresh(RefreshVisuals);
        }

        private void OnDestroy()
        {
            if (toggleButton != null)
                toggleButton.onClick.RemoveListener(OnToggleClicked);
        }

        [Inject]
        public void Construct(IInventoryManager inventoryManager, IContainerGridFactory gridFactory)
        {
            _presenter = new ContainerSectionPresenter(inventoryManager, gridFactory);
            _presenter.SetLayoutTargetGetter(() => transform.parent as RectTransform);
        }

        public void InitializeContainer(ItemTable itemTable, ContainerMetadata metadata, AbstractGrid gridPrefab)
        {
            _presenter.InitializeContainer(itemTable, metadata, gridPrefab, contentContainer);

            if (titleText != null)
                titleText.text = itemTable.ItemDataSo.DisplayName;

            _isExpanded = true;

            RefreshVisuals();

            if (!(_presenter.CalculateContentHeight() > 0))
                _presenter.RequestLateRefresh();
        }

        public void InitializeAsMainInventory(string sectionName, GridTable grid, AbstractGrid gridPrefab)
        {
            _presenter.InitializeAsMainInventory(grid, gridPrefab, contentContainer);

            if (titleText != null)
                titleText.text = sectionName;

            _isExpanded = true;

            RefreshVisuals();

            if (_presenter.CalculateContentHeight() <= 0)
                _presenter.RequestLateRefresh();
        }

        public void InitializeAsMainInventoryWithPanel(string sectionName, ContainerGridsData containerPanelPrefab, AbstractGrid fallbackGridPrefab, GridTable existingGrid)
        {
            _presenter.InitializeAsMainInventoryWithPanel(containerPanelPrefab, fallbackGridPrefab, existingGrid, contentContainer);

            if (titleText != null)
                titleText.text = sectionName;

            _isExpanded = true;

            RefreshVisuals();

            if (_presenter.CalculateContentHeight() <= 0)
                _presenter.RequestLateRefresh();
        }

        public void RefreshGridUI()
        {
            if (_presenter.RefreshGridUI())
                RefreshVisuals();
        }

        public void RefreshGridUISafe()
        {
            _presenter.RefreshGridUISafe(() => gameObject.activeInHierarchy, RefreshVisuals);
        }

        public void RefreshVisualsSafe()
        {
            _presenter.RefreshVisualsSafe(() => gameObject.activeInHierarchy, RefreshVisuals);
        }

        public void Close()
        {
            _presenter.Close();
            Destroy(gameObject);
        }

        private void RefreshVisuals()
        {
            if (_rectTransform == null || _layoutElement == null || _presenter == null)
                return;

            if (!_isExpanded)
            {
                _layoutElement.preferredHeight = collapsedHeight;
            }
            else
            {
                var contentHeight = _presenter.CalculateContentHeight();
                _layoutElement.preferredHeight = collapsedHeight + contentHeight;
            }

            _presenter.ForceUpdateParentLayout();
        }

        private void OnToggleClicked()
        {
            Toggle(!_isExpanded);
        }

        private void Toggle(bool expand)
        {
            _isExpanded = expand;

            if (contentContainer != null)
                contentContainer.gameObject.SetActive(_isExpanded);

            if (expandIcon != null)
                expandIcon.sprite = _isExpanded ? expandedIcon : collapsedIcon;

            if (gameObject.activeInHierarchy)
                RefreshAfterFrame().Forget();
            else
                RefreshVisuals();
        }

        private async UniTaskVoid RefreshAfterFrame()
        {
            await UniTask.DelayFrame(1);
            RefreshVisuals();
        }
    }
}
