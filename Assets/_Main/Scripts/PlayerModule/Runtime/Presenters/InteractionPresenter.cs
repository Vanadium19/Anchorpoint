using System;
using ComponentsModule;
using InventoryModule;
using UIModule;
using UtilsModule;
using Zenject;

namespace PlayerModule
{
    public class InteractionPresenter : IInitializable, IDisposable
    {
        private enum HintSource
        {
            None,
            Interactable,
            ExternalUI
        }

        private readonly PlayerInteractionController _controller;
        private readonly InteractionHUDView _view;

        private IInteractable _currentInteractable;
        private HintSource _activeSource;

        public InteractionPresenter(
            PlayerInteractionController controller,
            InteractionHUDView view)
        {
            _controller = controller;
            _view = view;
        }

        public void Initialize()
        {
            _view.Show(string.Empty);
            _view.Hide();
            _controller.InteractableHoverChanged += OnInteractableHoverChanged;
            _controller.ExternalUIHoverChanged += OnExternalUIHoverChanged;
            _controller.HoldProgressChanged += OnHoldProgressChanged;
        }

        public void Dispose()
        {
            _controller.InteractableHoverChanged -= OnInteractableHoverChanged;
            _controller.ExternalUIHoverChanged -= OnExternalUIHoverChanged;
            _controller.HoldProgressChanged -= OnHoldProgressChanged;
        }

        private void OnInteractableHoverChanged(IInteractable interactable)
        {
            _currentInteractable = interactable;

            if (interactable == null)
            {
                Hide(HintSource.Interactable);
                return;
            }

            _view.SetHoldProgress(0f, interactable is IHoldInteractable);
            Show(
                HintSource.Interactable,
                LocalizedText.GetFormatted(interactable.HintKey, interactable.DisplayName));
        }

        private void OnExternalUIHoverChanged(IExternalUI externalUI)
        {
            if (externalUI == null)
            {
                Hide(HintSource.ExternalUI);
                return;
            }

            _currentInteractable = null;
            _view.SetHoldProgress(0f, false);
            Show(
                HintSource.ExternalUI,
                LocalizedText.GetFormatted(InteractionHintKeys.Open, externalUI.DisplayName));
        }

        private void OnHoldProgressChanged(IHoldInteractable interactable, float progress)
        {
            if (_currentInteractable != interactable)
                return;

            _view.SetHoldProgress(progress, true);
        }

        private void Show(HintSource source, string message)
        {
            _activeSource = source;
            _view.Show(message);
        }

        private void Hide(HintSource source)
        {
            if (_activeSource != source)
                return;

            _activeSource = HintSource.None;
            _view.Hide();
        }
    }
}
