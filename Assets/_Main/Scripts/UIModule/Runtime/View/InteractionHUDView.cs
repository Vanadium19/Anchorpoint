using TMPro;
using UnityEngine;

namespace UIModule
{
    public class InteractionHUDView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text hintText;

        public void Show(string message)
        {
            hintText.text = message;
            canvasGroup.alpha = 1f;
        }

        public void Hide()
        {
            canvasGroup.alpha = 0f;
        }
    }
}