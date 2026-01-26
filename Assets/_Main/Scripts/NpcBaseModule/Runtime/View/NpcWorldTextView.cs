using TMPro;
using UnityEngine;

namespace NpcModule.Runtime
{
    public sealed class NpcWorldTextView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text text;

        private void Awake()
        {
            Hide();
            if (root == null)
            {
                var canvas = GetComponentInParent<Canvas>(true);
                root = canvas != null ? canvas.gameObject : gameObject;
            }

            if (text == null)
                text = GetComponentInChildren<TMP_Text>(true);
        }


        public void Show(string message)
        {
            if (text != null)
                text.SetText(message ?? string.Empty);

            if (root != null && !root.activeSelf)
                root.SetActive(true);
        }

        public void Hide()
        {
            if (root != null && root.activeSelf)
                root.SetActive(false);
        }
    }
}
