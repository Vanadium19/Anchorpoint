using System;

namespace ComponentsModule
{
    public sealed class InteractionFocusService : IInteractionFocusService
    {
        private IInteractable _currentInteractable;

        public event Action<IInteractable> InteractableChanged;

        public IInteractable CurrentInteractable => _currentInteractable;

        public void SetInteractable(IInteractable interactable)
        {
            if (_currentInteractable == interactable)
                return;

            _currentInteractable = interactable;
            InteractableChanged?.Invoke(interactable);
        }
    }
}
