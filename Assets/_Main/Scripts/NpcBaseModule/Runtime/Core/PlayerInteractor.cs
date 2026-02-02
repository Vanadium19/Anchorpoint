using UnityEngine;

namespace NpcBaseModule
{
    public sealed class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float maxDistance = 4f;
        [SerializeField] private LayerMask interactMask = ~0;
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private Transform interactorRoot;

        private void Awake()
        {
            playerCamera ??= Camera.main;
            interactorRoot ??= transform;
        }

        private void Update()
        {
            if (!Input.GetKeyDown(interactKey))
                return;

            if (playerCamera == null)
                return;

            var ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

            if (!Physics.Raycast(ray, out var hit, maxDistance, ~0, QueryTriggerInteraction.Ignore))
                return;

            var interactable = hit.collider.GetComponentInParent<IInteractable>();

            if (interactable == null)
                return;

            bool canInteract = interactable.CanInteract(interactorRoot);

            if (!canInteract)
                return;

            interactable.Interact(interactorRoot);
        }

        //FIXME: Remove unused method
        private bool TryGetInteractable(out IInteractable interactable)
        {
            interactable = null;

            var ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

            if (!Physics.Raycast(ray, out var hit, maxDistance, interactMask, QueryTriggerInteraction.Ignore))
                return false;

            interactable = hit.collider.GetComponentInParent<IInteractable>();

            return interactable != null;
        }
    }
}