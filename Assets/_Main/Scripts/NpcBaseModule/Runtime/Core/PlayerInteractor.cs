using UnityEngine;
using UnityEngine.InputSystem;

namespace NpcBaseModule
{
    public sealed class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float maxDistance = 4f;
        [SerializeField] private LayerMask interactMask = ~0;
        [SerializeField] private Transform interactorRoot;

        private InputAction _interactAction;

        private void Awake()
        {
            playerCamera ??= Camera.main;
            interactorRoot ??= transform;

            _interactAction = new InputAction(
                name: "Interact",
                type: InputActionType.Button,
                binding: "<Keyboard>/e"
            );
        }

        private void OnEnable() => _interactAction?.Enable();

        private void OnDisable() => _interactAction?.Disable();

        private void OnDestroy() => _interactAction?.Dispose();

        private void Update()
        {
            if (_interactAction == null || !_interactAction.WasPressedThisFrame())
                return;

            if (!TryGetInteractable(out var interactable))
                return;

            if (!interactable.CanInteract(interactorRoot))
                return;

            interactable.Interact(interactorRoot);
        }

        private bool TryGetInteractable(out IInteractable interactable)
        {
            interactable = null;

            if (playerCamera == null)
                return false;

            var ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

            if (!Physics.Raycast(ray, out var hit, maxDistance, interactMask, QueryTriggerInteraction.Collide))
                return false;

            interactable = hit.collider.GetComponentInParent<IInteractable>();

            return interactable != null;
        }
    }
}