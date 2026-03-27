using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using DG.Tweening;

namespace BuildingModule
{
    public class BuildingMenuView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform panelRect;
        [SerializeField] private GameObject itemPrefab;

        private BuildingMenuConfig _config;

        private RectTransform _container;
        private List<GameObject> _itemObjects = new();
        private List<BuildingMenuItem> _items = new();
        private int _selectedIndex;
        private float _scrollOffset;
        private float _targetScrollOffset;
        private float _scrollVelocity;
        private float _verticalOffset;
        private float _targetVerticalOffset;
        private float _verticalVelocity;

        private Vector2 _originalAnchoredPosition;
        private bool _isInitialized;
        private Tween _slideTween;

        [Inject]
        public void Construct(BuildingMenuConfig config)
        {
            _config = config;
        }

        private float ItemSpacing => _config?.ItemSpacing ?? 150f;
        private float SmoothTime => _config?.SmoothTime ?? 0.3f;
        private float ItemSize => _config?.ItemSize ?? 100f;
        private float AnimationOffset => _config?.AnimationOffset ?? 150f;
        private float MinItemScale => _config?.MinItemScale ?? 0.6f;
        private float MinItemAlpha => _config?.MinItemAlpha ?? 0.3f;

        public void Show()
        {
            AnimateShow(_config?.SlideOffset ?? 200f, _config?.SlideAnimationDuration ?? 0.3f);
            ResetVerticalPosition();
        }

        public void Hide()
        {
            AnimateHide(_config?.SlideOffset ?? 200f, _config?.SlideAnimationDuration ?? 0.3f);
        }

        public void AnimateShow(float slideOffset, float duration)
        {
            if (!_isInitialized && panelRect != null)
            {
                _originalAnchoredPosition = panelRect.anchoredPosition;
                _isInitialized = true;
            }

            _slideTween?.Kill();
            canvasGroup.alpha = 1f;
            
            if (panelRect != null)
            {
                panelRect.anchoredPosition = _originalAnchoredPosition + Vector2.down * slideOffset;
                _slideTween = panelRect
                    .DOAnchorPos(_originalAnchoredPosition, duration)
                    .SetEase(Ease.OutQuad);
            }
        }

        public void AnimateHide(float slideOffset, float duration)
        {
            if (!_isInitialized && panelRect != null)
            {
                _originalAnchoredPosition = panelRect.anchoredPosition;
                _isInitialized = true;
            }

            _slideTween?.Kill();

            if (panelRect != null)
            {
                _slideTween = panelRect
                    .DOAnchorPos(_originalAnchoredPosition + Vector2.down * slideOffset, duration)
                    .SetEase(Ease.InQuad)
                    .OnComplete(() =>
                    {
                        canvasGroup.alpha = 0f;
                        gameObject.SetActive(false);
                    });
            }
            else
            {
                canvasGroup.alpha = 0f;
                gameObject.SetActive(false);
            }
        }

        public void SetActive(bool active)
        {
            if (active && !gameObject.activeSelf)
                gameObject.SetActive(true);
        }

        public void SetItems(List<BuildingMenuItem> items, int selectedIndex)
        {
            SetItemsInternal(items, selectedIndex, false, 0f);
        }

        public void SetItemsWithAnimationUp(List<BuildingMenuItem> items, int selectedIndex)
        {
            SetItemsInternal(items, selectedIndex, true, AnimationOffset);
        }

        public void SetItemsWithAnimationDown(List<BuildingMenuItem> items, int selectedIndex)
        {
            SetItemsInternal(items, selectedIndex, true, -AnimationOffset);
        }

        private void SetItemsInternal(List<BuildingMenuItem> items, int selectedIndex, bool animateIn, float startY)
        {
            _items = items;
            _selectedIndex = selectedIndex;

            ClearItems();

            if (_container == null)
                _container = CreateDefaultContainer();

            var containerWidth = ItemSpacing;
            _container.sizeDelta = new Vector2(containerWidth, 0);

            if (!animateIn)
                startY = 0f;

            foreach (var item in items)
            {
                var itemGo = Instantiate(itemPrefab, _container);
                var itemRect = itemGo.transform as RectTransform;
                itemRect.anchorMin = new Vector2(0.5f, 0.5f);
                itemRect.anchorMax = new Vector2(0.5f, 0.5f);
                itemRect.pivot = new Vector2(0.5f, 0.5f);
                itemRect.anchoredPosition = new Vector2(0, startY);
                itemRect.sizeDelta = new Vector2(ItemSize, ItemSize);

                var itemView = itemGo.GetComponent<BuildingMenuItemView>();

                if (itemView != null)
                    itemView.SetData(item);

                _itemObjects.Add(itemGo);
            }

            _scrollOffset = selectedIndex * ItemSpacing;
            _targetScrollOffset = _scrollOffset;

            if (!animateIn)
            {
                _verticalOffset = 0f;
                _targetVerticalOffset = 0f;
                _verticalVelocity = 0f;
            }
            else
            {
                _verticalOffset = startY;
                _targetVerticalOffset = 0f;
                _verticalVelocity = 0f;
            }

            UpdateLayout();
        }

        private RectTransform CreateDefaultContainer()
        {
            var containerGo = new GameObject("ItemsContainer");
            var container = containerGo.AddComponent<RectTransform>();
            containerGo.AddComponent<CanvasRenderer>();
            container.SetParent(transform, false);
            container.anchorMin = new Vector2(0.5f, 0.5f);
            container.anchorMax = new Vector2(0.5f, 0.5f);
            container.pivot = new Vector2(0.5f, 0.5f);
            container.anchoredPosition = Vector2.zero;
            container.sizeDelta = new Vector2(0, 0);
            return container;
        }

        public void SelectNext()
        {
            if (_items.Count == 0)
                return;

            _selectedIndex = (_selectedIndex + 1) % _items.Count;
            _targetScrollOffset = _selectedIndex * ItemSpacing;
            UpdateSelection();
        }

        public void SelectPrevious()
        {
            if (_items.Count == 0)
                return;

            _selectedIndex = (_selectedIndex - 1 + _items.Count) % _items.Count;
            _targetScrollOffset = _selectedIndex * ItemSpacing;
            UpdateSelection();
        }

        public void AnimateInCategory()
        {
            _targetVerticalOffset = -AnimationOffset;
        }

        public void AnimateOutCategory()
        {
            _targetVerticalOffset = AnimationOffset;
        }

        public void ResetVerticalPosition()
        {
            _verticalOffset = 0f;
            _targetVerticalOffset = 0f;
        }

        public BuildingMenuItem GetSelectedItem()
        {
            if (_selectedIndex >= 0 && _selectedIndex < _items.Count)
                return _items[_selectedIndex];

            return null;
        }

        public int GetSelectedIndex() => _selectedIndex;

        private void Update()
        {
            if (_itemObjects.Count == 0)
                return;

            _scrollOffset = Mathf.SmoothDamp(_scrollOffset, _targetScrollOffset, ref _scrollVelocity, SmoothTime);
            _verticalOffset = Mathf.SmoothDamp(_verticalOffset, _targetVerticalOffset, ref _verticalVelocity, SmoothTime);
            UpdateLayout();
        }

        private void UpdateLayout()
        {
            if (_itemObjects.Count == 0)
                return;

            for (int i = 0; i < _itemObjects.Count; i++)
            {
                var itemGo = _itemObjects[i];
                var rect = itemGo.transform as RectTransform;

                var scrollDelta = _scrollOffset - _selectedIndex * ItemSpacing;
                var targetX = (i - _selectedIndex) * ItemSpacing - scrollDelta;

                rect.anchoredPosition = new Vector2(targetX, _verticalOffset);

                var distance = Mathf.Abs(i * ItemSpacing - _scrollOffset);
                var scale = Mathf.Clamp(1f - distance / (ItemSpacing * 2f), MinItemScale, 1f);
                rect.localScale = Vector3.one * scale;

                var itemView = itemGo.GetComponent<BuildingMenuItemView>();

                if (itemView != null)
                {
                    var alpha = Mathf.Clamp(1f - distance / (ItemSpacing * 2f), MinItemAlpha, 1f);
                    itemView.SetAlpha(alpha);
                    itemView.SetSelected(i == _selectedIndex);
                }
            }
        }

        private void UpdateSelection()
        {
            for (int i = 0; i < _itemObjects.Count; i++)
            {
                var itemView = _itemObjects[i].GetComponent<BuildingMenuItemView>();
                
                if (itemView != null)
                    itemView.SetSelected(i == _selectedIndex);
            }
        }

        private void ClearItems()
        {
            foreach (var itemGo in _itemObjects)
            {
                if (itemGo != null)
                    Destroy(itemGo);
            }
            _itemObjects.Clear();
        }
    }
}