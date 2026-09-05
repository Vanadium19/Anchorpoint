using TMPro;
using UnityEngine;

namespace RandomEventsModule
{
    /// <summary>MVP view for a single HUD notification message.</summary>
    public class RandomEventNotificationView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text messageText;

        private void Awake()
        {
            Hide();
        }

        /// <summary>Displays the given message at full opacity.</summary>
        public void Show(string message)
        {
            messageText.text = message;
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        /// <summary>Hides the message.</summary>
        public void Hide()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }
}
