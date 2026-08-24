using UnityEngine;

namespace ComponentsModule
{
    public interface IInteractable
    {
        string DisplayName { get; }

        string HintKey { get; }

        bool CanInteract(Transform interactor);

        void Interact(Transform interactor);
    }
}
