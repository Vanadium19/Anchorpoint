using TMPro;
using UnityEngine;

namespace NpcBaseModule
{
    public sealed class NpcWorldTextView : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text text;

        private void Awake() => Initialize();

        public void Show(string message)
        {
            Initialize();

            if (root == null)
                return;

            root.SetActive(true);

            if (canvas != null)
                canvas.enabled = true;

            if (text == null)
                return;

            text.gameObject.SetActive(true);
            text.SetText(message);
        }

        public void Hide()
        {
            Initialize();

            if (root != null)
                root.SetActive(false);
        }

        private void Initialize()
        {
            if (canvas == null)
                canvas = GetComponentInParent<Canvas>(true);

            if (root == null)
                root = canvas != null ? canvas.gameObject : gameObject;

            if (text == null)
                text = GetComponentInChildren<TMP_Text>(true);
        }
    }
}