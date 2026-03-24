using UnityEngine;

namespace InventoryModule.ContextMenu.UI
{
    public class ContextMenuPresenter : MonoBehaviour
    {
        [SerializeField] private ContextMenuView view;

        private IContextActionService _actionService;
        private IContextMenuStateService _stateService;

        public void Initialize(IContextActionService actionService, IContextMenuStateService stateService)
        {
            _actionService = actionService;
            _stateService = stateService;

            view.Initialize(_stateService);
        }

        public void ShowForItem(ItemTable item, Vector2 screenPosition)
        {
            var actions = _actionService.GetActions(item);

            if (actions.Count == 0)
                return;

            view.Show(screenPosition, actions, null);
        }
    }
}