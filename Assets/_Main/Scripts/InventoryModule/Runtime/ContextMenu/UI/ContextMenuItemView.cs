using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;

namespace InventoryModule.ContextMenu.UI
{
    public class ContextMenuItemView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI labelText;
        [SerializeField] private Button button;

        private IContextAction _action;
        private Action<IContextAction> _onExecuted;

        private void Awake()
        {
            button.onClick.AddListener(OnClicked);
        }

        public void Initialize(IContextAction action, Action<IContextAction> onExecuted)
        {
            _action = action;
            _onExecuted = onExecuted;

            labelText.text = action.DisplayName;
        }

        private void OnClicked()
        {
            _onExecuted?.Invoke(_action);
        }

        public class Factory : PlaceholderFactory<ContextMenuItemView>
        {
        }
    }
}
