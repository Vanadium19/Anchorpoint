using UnityEngine;
using UnityEngine.AI;

namespace NpcBaseModule
{
    public sealed class FriendlyNpcView : MonoBehaviour, IInteractable
    {
        [SerializeField] private FriendlyNpcSettings settings;
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private Transform headPoint;
        [SerializeField] private NpcWorldTextView worldText;

        private FriendlyNpcController _controller;

        public Transform Transform => transform;
        public float InteractionRadius => settings.InteractionRadius;

        public FriendlyNpcSettings Settings => settings;
        public NavMeshAgent Agent => agent;
        public Transform HeadPoint => headPoint;
        public NpcWorldTextView WorldText => worldText;

        private void Awake()
        {
            agent ??= GetComponent<NavMeshAgent>();
            worldText ??= GetComponentInChildren<NpcWorldTextView>(true);
        }

        public void SetController(FriendlyNpcController controller) => _controller = controller;

        public bool CanInteract(Transform interactor)
        {
            if (interactor == null)
                return false;

            Vector3 interactorPosition = interactor.position;
            Vector3 npcPosition = transform.position;

            interactorPosition.y = 0f;
            npcPosition.y = 0f;

            float distance = Vector3.Distance(interactorPosition, npcPosition);
            return distance <= settings.InteractionRadius;
        }

        public void Interact(Transform interactor) => _controller?.OnInteract(interactor);
    }
}