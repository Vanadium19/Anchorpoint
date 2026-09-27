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

        [Inject(Optional = true)] private IInteractionFocusService _interactionFocusService;

        private IInteractable _currentHoveredInteractable;
        private IExternalUI _currentHoveredExternalUI;
        private Collider _lastHitCollider;
        private float _holdElapsedTime;
        private bool _lastGateAllowed;
        private bool _isPaused;
        private bool _waitForHoldRelease;

        public event Action<IInteractable> InteractableHoverChanged;
        public event Action<IExternalUI> ExternalUIHoverChanged;
        public event Action<IHoldInteractable, float> HoldProgressChanged;

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
                ClearInteraction();
        }

        public void Tick()
        {
            if (_isPaused)
                return;

            if (_input.IsBuildMode || _input.IsUIMode)
            {
                ClearInteraction();
                return;
            }

            var ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            if (!Physics.Raycast(ray, out var hit, _config.InteractionDistance, _config.InteractionLayer))
            {
                ClearInteraction();
                return;
            }

            var isGateAllowed = InteractionGateUtility.IsAllowed(hit.collider.transform);

            if (hit.collider != _lastHitCollider || isGateAllowed != _lastGateAllowed)
            {
                _lastHitCollider = hit.collider;
                _lastGateAllowed = isGateAllowed;
                FindAndCacheTargets(hit.collider, isGateAllowed);
            }

            HandleInteractionInput();
        }

        private void FindAndCacheTargets(Collider collider, bool isGateAllowed)
        {
            if (!isGateAllowed)
            {
                UpdateHover(FindGateBypassInteractable(collider), null);
                return;
            }

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
                    var gateBypass = interactable as IInteractionGateBypass;

                    if (gateBypass == null || gateBypass.CanBypassInteractionGate)
                    {
                        UpdateHover(interactable, null);
                        return;
                    }
                }

                current = current.parent;
            }

            UpdateHover(null, null);
        }

        private IInteractable FindGateBypassInteractable(Collider collider)
        {
            var current = collider.transform;

            while (current != null)
            {
                var gateBypass = current.GetComponent<IInteractionGateBypass>();

                if (gateBypass != null && gateBypass.CanBypassInteractionGate)
                    return current.GetComponent<IInteractable>();

                current = current.parent;
            }

            return null;
        }

        private void HandleInteractionInput()
        {
            if (_currentHoveredExternalUI != null)
            {
                CancelHold();

                if (_input.IsInteractPressed)
                    _externalUIManager.Open(_currentHoveredExternalUI);

                return;
            }

            if (_currentHoveredInteractable == null)
            {
                CancelHold();
                return;
            }

            if (_currentHoveredInteractable is IHoldInteractable holdInteractable)
            {
                HandleHoldInteraction(holdInteractable);
                return;
            }

            CancelHold();

            if (!_input.IsInteractPressed)
                return;

            if (!_currentHoveredInteractable.CanInteract(_playerTransform))
                return;

            _currentHoveredInteractable.Interact(_playerTransform);
        }

        private void HandleHoldInteraction(IHoldInteractable holdInteractable)
        {
            if (!_input.IsInteractHeld)
            {
                ResetHoldAfterRelease(holdInteractable);
                return;
            }

            if (_waitForHoldRelease)
                return;

            if (!holdInteractable.CanInteract(_playerTransform))
            {
                ResetHoldProgress(holdInteractable);
                return;
            }

            var holdDuration = Mathf.Max(holdInteractable.HoldDuration, 0f);

            if (holdDuration <= 0f)
            {
                holdInteractable.Interact(_playerTransform);
                _waitForHoldRelease = true;
                HoldProgressChanged?.Invoke(holdInteractable, 1f);
                return;
            }

            _holdElapsedTime += Time.deltaTime;
            var progress = Mathf.Clamp01(_holdElapsedTime / holdDuration);
            HoldProgressChanged?.Invoke(holdInteractable, progress);

            if (progress < 1f)
                return;

            holdInteractable.Interact(_playerTransform);
            _holdElapsedTime = 0f;
            _waitForHoldRelease = true;
        }

        private void ResetHoldAfterRelease(IHoldInteractable holdInteractable)
        {
            if (_holdElapsedTime <= 0f && !_waitForHoldRelease)
                return;

            _holdElapsedTime = 0f;
            _waitForHoldRelease = false;
            HoldProgressChanged?.Invoke(holdInteractable, 0f);
        }

        private void ResetHoldProgress(IHoldInteractable holdInteractable)
        {
            if (_holdElapsedTime <= 0f)
                return;

            _holdElapsedTime = 0f;
            HoldProgressChanged?.Invoke(holdInteractable, 0f);
        }

        private void CancelHold()
        {
            if (_holdElapsedTime > 0f && _currentHoveredInteractable is IHoldInteractable holdInteractable)
                HoldProgressChanged?.Invoke(holdInteractable, 0f);

            _holdElapsedTime = 0f;
            _waitForHoldRelease = false;
        }

        private void UpdateHover(IInteractable newInteractable, IExternalUI newExternalUI)
        {
            if (_currentHoveredInteractable == newInteractable && _currentHoveredExternalUI == newExternalUI)
                return;

            CancelHold();
            _currentHoveredInteractable = newInteractable;
            _currentHoveredExternalUI = newExternalUI;

            InteractableHoverChanged?.Invoke(newInteractable);
            _interactionFocusService?.SetInteractable(newInteractable);
            ExternalUIHoverChanged?.Invoke(newExternalUI);
        }

        private void ClearInteraction()
        {
            if (_lastHitCollider == null && _currentHoveredInteractable == null && _currentHoveredExternalUI == null)
                return;

            _lastHitCollider = null;
            _lastGateAllowed = false;
            UpdateHover(null, null);
        }
    }
}
