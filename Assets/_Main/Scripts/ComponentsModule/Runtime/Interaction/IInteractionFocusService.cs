using System;

namespace ComponentsModule
{
    public interface IInteractionFocusService
    {
        event Action<IInteractable> InteractableChanged;

        IInteractable CurrentInteractable { get; }

        void SetInteractable(IInteractable interactable);
    }
}
