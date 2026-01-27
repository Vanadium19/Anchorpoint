using TMPro;
using UnityEngine;

namespace NpcBaseModule
{
    public sealed class NpcWorldTextView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text text;

        private void Awake()
        {
            if (root == null)
            {
                var canvas = GetComponentInParent<Canvas>(true);
                root = canvas != null ? canvas.gameObject : gameObject;
            }

            text ??= GetComponentInChildren<TMP_Text>(true);
        }

        private void Start() => Hide();

        public void Show(string message)
        {
            text?.SetText(message);
            root?.SetActive(true);
        }

        public void Hide() => root?.SetActive(false);
    }
}