using UnityEngine;
using Zenject;
using InventoryModule.ContextMenu;

namespace InventoryModule.ContextMenu.UI
{
    public class ContextMenuPresenter : MonoBehaviour
    {
        [SerializeField] private ContextMenuView view;

        private IContextActionService _actionService;

        [Inject]
        private void Construct(IContextActionService actionService)
        {
            _actionService = actionService;
        }

        public void Initialize(IContextActionService actionService)
        {
            _actionService = actionService;
        }

        private void Awake()
        {
        }

        public void ShowForItem(ItemTable item, Vector2 screenPosition)
        {
            var actions = _actionService.GetActions(item);

            if (actions.Count == 0)
                return;

            view.Show(screenPosition, actions, null);
        }

        public void Hide()
        {
            view.Hide();
        }
    }
}
