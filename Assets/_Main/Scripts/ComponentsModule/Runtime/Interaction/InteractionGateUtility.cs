using UnityEngine;

namespace ComponentsModule
{
    public static class InteractionGateUtility
    {
        public static bool IsAllowed(Transform transform)
        {
            var current = transform;

            while (current != null)
            {
                var interactionGate = current.GetComponent<IInteractionGate>();

                if (interactionGate != null && !interactionGate.CanInteract)
                    return false;

                current = current.parent;
            }

            return true;
        }
    }
}
