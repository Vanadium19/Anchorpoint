using System;
using UnityEngine;

namespace ExtractionModule.View
{
    public class ExtractionZoneView : MonoBehaviour
    {
        public event Action<bool> PlayerPresenceChanged;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                PlayerPresenceChanged?.Invoke(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                PlayerPresenceChanged?.Invoke(false);
            }
        }
    }
}