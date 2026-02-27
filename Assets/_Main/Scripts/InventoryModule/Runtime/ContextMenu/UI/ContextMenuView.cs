using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InventoryModule.ContextMenu.UI
{
    public class ContextMenuView : MonoBehaviour
    {
        private static ContextMenuView _activeMenu;
        private static GameObject _sharedBlockerInstance;

        [Header("References")]
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Transform itemsContainer;
        [SerializeField] private ContextMenuItemView itemPrefab;

        [Header("Blocker")]
        [SerializeField] private GameObject blockerPrefab;

        private readonly List<ContextMenuItemView> _items = new List<ContextMenuItemView>();
        private Action _onHide;

        public void Show(Vector2 screenPosition, IReadOnlyList<IContextAction> actions, Action onHide)
        {
            if (_activeMenu != null && _activeMenu != this)
            {
                _activeMenu.Hide();
            }

            _activeMenu = this;
            _onHide = onHide;
            ClearItems();
            gameObject.SetActive(true);

            foreach (var action in actions)
            {
                var itemView = Instantiate(itemPrefab, itemsContainer);
                itemView.Initialize(action, OnActionExecuted);
                _items.Add(itemView);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            rectTransform.position = screenPosition;

            EnsureBlockerExists();
            UpdateBlockerSibling();
        }

        public void Hide()
        {
            ClearItems();
            gameObject.SetActive(false);

            if (_activeMenu == this)
            {
                _activeMenu = null;
                if (_sharedBlockerInstance != null)
                {
                    _sharedBlockerInstance.SetActive(false);
                }
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
                {
                    Destroy(item.gameObject);
                }
            }
            _items.Clear();
        }

        private void EnsureBlockerExists()
        {
            if (blockerPrefab == null) return;

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            if (_sharedBlockerInstance == null)
            {
                _sharedBlockerInstance = Instantiate(blockerPrefab, canvas.transform);
            }

            _sharedBlockerInstance.SetActive(true);

            var button = _sharedBlockerInstance.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(Hide);
            }
        }

        private void UpdateBlockerSibling()
        {
            if (_sharedBlockerInstance == null) return;

            transform.SetAsLastSibling();
            _sharedBlockerInstance.transform.SetSiblingIndex(transform.GetSiblingIndex() - 1);
        }
    }
}
