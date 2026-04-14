using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace InventoryModule.ContextMenu.UI
{
    public class ContextMenuView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Transform itemsContainer;
        [SerializeField] private ContextMenuItemView itemPrefab;

        [Header("Blocker")]
        [SerializeField] private GameObject blockerPrefab;

        private readonly List<ContextMenuItemView> _items = new();

        private IContextMenuStateService _stateService;
        private DiContainer _diContainer;
        private Action _onHide;

        [Inject]
        public void Construct(DiContainer container)
        {
            _diContainer = container;
        }

        public void Initialize(IContextMenuStateService stateService)
        {
            _stateService = stateService;
        }

        public void Show(Vector2 screenPosition, IReadOnlyList<IContextAction> actions, Action onHide)
        {
            if (_stateService.ActiveMenu != null && _stateService.ActiveMenu != this)
                _stateService.ActiveMenu.Hide();

            _stateService.ActiveMenu = this;
            _onHide = onHide;

            ClearItems();
            gameObject.SetActive(true);

            foreach (var action in actions)
            {
                var itemView = _diContainer != null
                    ? _diContainer.InstantiatePrefabForComponent<ContextMenuItemView>(itemPrefab, itemsContainer)
                    : Instantiate(itemPrefab, itemsContainer);
                itemView.Initialize(action, OnActionExecuted);
                _items.Add(itemView);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            rectTransform.position = screenPosition;

            EnsureBlockerExists();
            UpdateBlockerSibling();
        }

        private void Hide()
        {
            ClearItems();
            gameObject.SetActive(false);

            if (_stateService.ActiveMenu == this)
            {
                _stateService.ActiveMenu = null;

                if (_stateService.BlockerInstance != null)
                    _stateService.BlockerInstance.SetActive(false);
            }

            _onHide?.Invoke();
            _onHide = null;
        }

        private void OnActionExecuted(IContextAction action)
        {
            action.Execute();
            Hide();
        }

        private void ClearItems()
        {
            foreach (var item in _items)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }

            _items.Clear();
        }

        private void EnsureBlockerExists()
        {
            if (blockerPrefab == null)
                return;

            var canvas = GetComponentInParent<Canvas>();

            if (canvas == null)
                return;

            if (_stateService.BlockerInstance == null)
                _stateService.BlockerInstance = Instantiate(blockerPrefab, canvas.transform);

            _stateService.BlockerInstance.SetActive(true);

            var button = _stateService.BlockerInstance.GetComponent<Button>();

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(Hide);
            }
        }

        private void UpdateBlockerSibling()
        {
            if (_stateService.BlockerInstance == null)
                return;

            transform.SetAsLastSibling();
            _stateService.BlockerInstance.transform.SetSiblingIndex(transform.GetSiblingIndex() - 1);
        }
    }
}