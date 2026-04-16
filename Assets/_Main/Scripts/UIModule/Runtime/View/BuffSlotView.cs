using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UIModule
{
    public class BuffSlotView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Image durationRing;
        [SerializeField] private TextMeshProUGUI durationText;
        [SerializeField] private Color activeColor = Color.green;
        [SerializeField] private Color expiringColor = Color.red;

        private float _maxDuration;
        private float _remainingTime;

        public void SetIcon(Sprite icon)
        {
            if (iconImage != null)
                iconImage.sprite = icon;
        }

        public void SetDuration(float remaining, float max)
        {
            _maxDuration = max;
            _remainingTime = remaining;
            UpdateVisuals();
        }

        public void UpdateRemaining(float remaining)
        {
            _remainingTime = remaining;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (durationRing != null)
            {
                var fillAmount = _maxDuration > 0 ? _remainingTime / _maxDuration : 0f;
                durationRing.fillAmount = fillAmount;
                durationRing.color = fillAmount > 0.3f ? activeColor : expiringColor;
            }

            if (durationText != null)
            {
                var seconds = Mathf.CeilToInt(_remainingTime);
                durationText.text = seconds.ToString();
            }
        }
    }
}
