using System;
using InputModule;
using UnityEngine;
using Zenject;

namespace NpcBaseModule
{
    public sealed class NpcDialoguePresenter : MonoBehaviour
    {
        [SerializeField] private NpcDialogueView view;
        [SerializeField] private DialogueCameraFocus cameraFocus;

        private IInputService _inputService;
        private Action<int> _answerSelected;
        private bool _isOpen;

        public bool IsOpen => _isOpen;

        [Inject]
        public void Construct(IInputService inputService) => _inputService = inputService;

        private void Awake()
        {
            view ??= GetComponent<NpcDialogueView>();

            if (view != null)
                view.AnswerClicked += OnAnswerClicked;

            view?.Hide();
        }

        private void OnDestroy()
        {
            if (view != null)
                view.AnswerClicked -= OnAnswerClicked;
        }

        public void Begin(FriendlyNpcView npcView, Action<int> answerSelected)
        {
            _answerSelected = answerSelected;
            _isOpen = true;

            _inputService?.SetUIMode(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (npcView.HeadPoint != null)
            {
                cameraFocus?.Focus(npcView.HeadPoint);
            }
            else
            {
                var focusPosition = npcView.Transform.position + Vector3.up * 1.6f;
                cameraFocus?.Focus(focusPosition);
            }

            view?.Show();
        }

        public void Render(
            string npcName,
            int relationshipLevel,
            string message,
            System.Collections.Generic.IReadOnlyList<NpcDialogueAnswer> answers,
            bool hasNextLine)
        {
            view?.Render(npcName, relationshipLevel, message, answers, hasNextLine);
        }

        public void Close()
        {
            _isOpen = false;
            _answerSelected = null;

            cameraFocus?.Clear();
            view?.Hide();

            _inputService?.SetUIMode(false);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnAnswerClicked(int answerIndex) => _answerSelected?.Invoke(answerIndex);
    }
}