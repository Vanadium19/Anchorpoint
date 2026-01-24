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
            if (root == null)
                root = gameObject;

            Hide();
        }

        public void Show(string message)
        {
            if (text != null)
                text.text = message ?? string.Empty;

            if (root != null)
                root.SetActive(true);
        }

        public void Hide()
        {
            if (root != null)
                root.SetActive(false);
        }
    }
}
