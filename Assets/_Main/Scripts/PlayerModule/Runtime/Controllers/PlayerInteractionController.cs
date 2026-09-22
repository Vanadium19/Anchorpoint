using System;
using BaseModule;
using ComponentsModule;
using InputModule;
using InventoryModule;
using UIModule;
using UnityEngine;
using Zenject;

namespace PlayerModule
{
    public sealed class PlayerInteractionController : IInitializable, ITickable, IDisposable, IPausable
    {
        private readonly IInputMap _input;
        private readonly ExternalUIManager _externalUIManager;
        private readonly Camera _camera;
        private readonly Transform _playerTransform;
        private readonly PlayerConfig _config;
        private readonly IPauseManager _pauseManager;

        public event Action<IInteractable> InteractableHoverChanged;

        public event Action<IExternalUI> ExternalUIHoverChanged;

        private IInteractable _currentHoveredInteractable;
        private IExternalUI _currentHoveredExternalUI;
        private Collider _lastHitCollider;
        private bool _isPaused;

        public PlayerInteractionController(
            IInputMap input,
            ExternalUIManager externalUIManager,
            Camera camera,
            Transform playerTransform,
            PlayerConfig config,
            IPauseManager pauseManager)
        {
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _externalUIManager = externalUIManager;
            _camera = camera;
            _playerTransform = playerTransform;
            _config = config;
            _pauseManager = pauseManager;
        }

        public void Initialize() => _pauseManager.Register(this);

        public void Dispose() => _pauseManager.Unregister(this);

        public void SetPaused(bool isPaused)
        {
            _isPaused = isPaused;

            if (isPaused)
            {
                _lastHitCollider = null;
                UpdateHover(null, null);
            }
        }

        public void Tick()
        {
            if (_isPaused)
                return;

            var ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            if (Physics.Raycast(ray, out var hit, _config.InteractionDistance, _config.InteractionLayer))
            {
                if (hit.collider != _lastHitCollider)
                {
                    _lastHitCollider = hit.collider;
                    FindAndCacheTargets(hit.collider);
                }

                if (_input.IsInteractPressed)
                    HandleInteraction();
            }
            else
            {
                if (_currentHoveredInteractable != null || _currentHoveredExternalUI != null)
                    UpdateHover(null, null);

                _lastHitCollider = null;
            }
        }

        private void FindAndCacheTargets(Collider collider)
        {
            var current = collider.transform;

            while (current != null)
            {
                var externalUI = current.GetComponent<IExternalUI>();

                if (externalUI != null)
                {
                    UpdateHover(null, externalUI);
                    return;
                }

                var interactable = current.GetComponent<IInteractable>();

                if (interactable != null)
                {
                    UpdateHover(interactable, null);
                    return;
                }

                current = current.parent;
            }

            UpdateHover(null, null);
        }

        private void HandleInteraction()
        {
            if (_currentHoveredExternalUI != null)
            {
                _externalUIManager.Open(_currentHoveredExternalUI);
                return;
            }

            if (_currentHoveredInteractable == null)
                return;

            if (!_currentHoveredInteractable.CanInteract(_playerTransform))
                return;

            _currentHoveredInteractable.Interact(_playerTransform);
        }

        private void UpdateHover(IInteractable newInteractable, IExternalUI newExternalUI)
        {
            if (_currentHoveredInteractable == newInteractable && _currentHoveredExternalUI == newExternalUI)
                return;

            _currentHoveredInteractable = newInteractable;
            _currentHoveredExternalUI = newExternalUI;

            InteractableHoverChanged?.Invoke(newInteractable);
            ExternalUIHoverChanged?.Invoke(newExternalUI);
        }
    }
}
