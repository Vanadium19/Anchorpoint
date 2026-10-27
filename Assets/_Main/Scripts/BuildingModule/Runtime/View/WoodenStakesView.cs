using System;
using UnityEngine;

namespace BuildingModule
{
    public sealed class WoodenStakesView : MonoBehaviour
    {
        [SerializeField] private Collider triggerCollider;

        public event Action<Collider> TargetEntered;
        public event Action<Collider> TargetExited;

        private void OnTriggerEnter(Collider other) => TargetEntered?.Invoke(other);

        private void OnTriggerExit(Collider other) => TargetExited?.Invoke(other);

        public void SetTriggerEnabled(bool isEnabled)
        {
            if (triggerCollider != null)
                triggerCollider.enabled = isEnabled;
        }
    }
}
