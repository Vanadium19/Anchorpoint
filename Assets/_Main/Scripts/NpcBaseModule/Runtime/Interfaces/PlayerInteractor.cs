using UnityEngine;
using NpcModule.Runtime;

namespace NpcModule.Runtime
{
    public sealed class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float maxDistance = 4f;
        [SerializeField] private LayerMask interactMask = ~0;
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        private void Awake()
        {
            if (playerCamera == null)
                playerCamera = Camera.main;
        }

        private void Update()
        {
            if (playerCamera == null)
                return;

            if (!Input.GetKeyDown(interactKey))
                return;

            if (!TryGetInteractable(out var interactable))
                return;

            if (!interactable.CanInteract(transform))
                return;


            interactable.Interact(transform);
        }

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
