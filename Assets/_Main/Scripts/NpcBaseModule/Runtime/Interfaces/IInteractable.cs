using UnityEngine;

public interface IInteractable
{
    Transform Transform { get; }
    float InteractionRadius { get; }
    bool CanInteract(Transform interactor);
    void Interact(Transform interactor);
}
