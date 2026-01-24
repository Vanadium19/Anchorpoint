using UnityEngine;
using UnityEngine.AI;

namespace NpcModule.Runtime
{
    public sealed class FriendlyNpcView : MonoBehaviour, IInteractable
    {
        [SerializeField] private FriendlyNpcSettings settings;
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private Transform headPoint;
        [SerializeField] private NpcWorldTextView worldText;

        private FriendlyNpcController _controller;

        public Transform Transform => transform;
        public float InteractionRadius => settings.interactionRadius;

        public FriendlyNpcSettings Settings => settings;
        public NavMeshAgent Agent => agent;
        public Transform HeadPoint => headPoint;
        public NpcWorldTextView WorldText => worldText;

        private void Awake()
        {
            if (agent == null)
                agent = GetComponent<NavMeshAgent>();

            if (worldText == null)
                worldText = GetComponentInChildren<NpcWorldTextView>(true);
        }

        public void SetController(FriendlyNpcController controller)
        {
            _controller = controller;
        }

        public bool CanInteract(Transform interactor)
        {
            if (interactor == null)
                return false;

            Vector3 a = interactor.position;
            Vector3 b = transform.position;

            a.y = 0f;
            b.y = 0f;

            float dist = Vector3.Distance(a, b);
            return dist <= settings.interactionRadius;
        }


        public void Interact(Transform interactor)
        {
            if (_controller == null)
                return;

            _controller.OnInteract(interactor);
        }
    }
}
