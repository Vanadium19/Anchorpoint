using PlayerModule;
using UnityEngine;
using Zenject;

namespace EvacuationModule
{
    public class EvacuationController : MonoBehaviour
    {
        private IEvacuationService _evacuationService;

        [Inject]
        public void Construct(IEvacuationService evacuationService)
        {
            _evacuationService = evacuationService;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerProvider player))
                _evacuationService.ChangeZoneState(true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out PlayerProvider player))
                _evacuationService.ChangeZoneState(false);
        }
    }
}