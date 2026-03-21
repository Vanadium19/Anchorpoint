using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace InventoryModule.ContextMenu.UI
{
    public class ContextMenuItemView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI labelText;
        [SerializeField] private Button button;

        private IContextAction _action;
        private Action<IContextAction> _onExecuted;

        private void Start() => button.onClick.AddListener(OnClicked);

        private void OnDestroy() => button.onClick.RemoveListener(OnClicked);

        public void Initialize(IContextAction action, Action<IContextAction> onExecuted)
        {
            _action = action;
            _onExecuted = onExecuted;

            labelText.text = action.DisplayName;
        }

        private void OnClicked() => _onExecuted?.Invoke(_action);
    }
}