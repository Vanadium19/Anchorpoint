using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIModule
{
    public class InteractionHUDView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text hintText;
        [SerializeField] private Image holdProgressFillImage;
        [SerializeField] private TMP_Text holdProgressText;

        private string _message = string.Empty;
        private float _holdProgress;
        private bool _isHoldProgressVisible;

        public void Show(string message)
        {
            _message = message;
            canvasGroup.alpha = 1f;
            Render();
        }

        public void Hide()
        {
            canvasGroup.alpha = 0f;
            SetHoldProgress(0f, false);
        }

        public void SetHoldProgress(float progress, bool isVisible)
        {
            _holdProgress = Mathf.Clamp01(progress);
            _isHoldProgressVisible = isVisible;
            Render();
        }

        private void Render()
        {
            var progressPercent = Mathf.RoundToInt(_holdProgress * 100f);

            if (holdProgressFillImage != null)
            {
                holdProgressFillImage.gameObject.SetActive(_isHoldProgressVisible);
                holdProgressFillImage.fillAmount = _holdProgress;
            }

            if (holdProgressText != null)
            {
                holdProgressText.gameObject.SetActive(_isHoldProgressVisible);
                holdProgressText.text = $"{progressPercent}%";
            }

            if (hintText == null)
                return;

            if (_isHoldProgressVisible && holdProgressFillImage == null && holdProgressText == null)
            {
                hintText.text = $"{_message}\n{progressPercent}%";
                return;
            }

            hintText.text = _message;
        }
    }
}
