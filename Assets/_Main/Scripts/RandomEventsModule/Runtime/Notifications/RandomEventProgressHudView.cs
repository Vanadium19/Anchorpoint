using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RandomEventsModule
{
    /// <summary>MVP view for a HUD hint plus a fill bar, for actions that need the player to hold a button near something for a while.</summary>
    public class RandomEventProgressHudView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text hintText;
        [SerializeField] private Slider progressSlider;

        private void Awake()
        {
            Hide();
        }

        /// <summary>Shows the hint text; progress is left as it was until <see cref="SetProgress"/> is called.</summary>
        public void Show(string hint)
        {
            hintText.text = hint;
            canvasGroup.alpha = 1f;
        }

        /// <summary>Sets the fill bar to the given normalized progress (0..1).</summary>
        public void SetProgress(float normalizedProgress) => progressSlider.value = Mathf.Clamp01(normalizedProgress);

        /// <summary>Hides the hint and resets the fill bar.</summary>
        public void Hide()
        {
            canvasGroup.alpha = 0f;
            progressSlider.value = 0f;
        }
    }
}
