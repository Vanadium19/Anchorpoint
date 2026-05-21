using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NpcBaseModule
{
    public sealed class NpcDialogueView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text npcNameText;
        [SerializeField] private TMP_Text relationshipText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Transform answersRoot;
        [SerializeField] private Button answerButtonPrefab;
        [SerializeField] private string continueButtonText = "Далее";
        [SerializeField] private string endButtonText = "Закончить";

        private readonly List<Button> _buttons = new();

        public event Action<int> AnswerClicked;

        private void Awake()
        {
            if (root == null)
                root = gameObject;

            Hide();
        }

        public void Render(
            string npcName,
            int relationshipLevel,
            string message,
            IReadOnlyList<NpcDialogueAnswer> answers,
            bool hasNextLine)
        {
            Show();

            if (npcNameText != null)
                npcNameText.SetText(npcName);

            if (relationshipText != null)
                relationshipText.SetText($"Отношения: {relationshipLevel}");

            if (messageText != null)
                messageText.SetText(message);

            ClearButtons();

            if (answers != null && answers.Count > 0)
            {
                for (var i = 0; i < answers.Count; i++)
                    CreateButton(answers[i].Text, i);

                return;
            }

            var buttonText = hasNextLine ? continueButtonText : endButtonText;
            CreateButton(buttonText, -1);
        }

        public void Show()
        {
            if (root == null)
                root = gameObject;

            root.SetActive(true);
        }

        public void Hide()
        {
            ClearButtons();

            if (root == null)
                root = gameObject;

            root.SetActive(false);
        }

        private void CreateButton(string buttonText, int answerIndex)
        {
            if (answerButtonPrefab == null || answersRoot == null)
                return;

            var button = Instantiate(answerButtonPrefab, answersRoot);
            button.gameObject.SetActive(true);

            var text = button.GetComponentInChildren<TMP_Text>(true);

            if (text != null)
                text.SetText(buttonText);

            button.onClick.AddListener(() => AnswerClicked?.Invoke(answerIndex));
            _buttons.Add(button);
        }

        private void ClearButtons()
        {
            for (var i = 0; i < _buttons.Count; i++)
            {
                if (_buttons[i] != null)
                    Destroy(_buttons[i].gameObject);
            }

            _buttons.Clear();
        }
    }
}