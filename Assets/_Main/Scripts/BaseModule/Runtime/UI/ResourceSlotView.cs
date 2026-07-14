using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BaseModule
{
    public class ResourceSlotView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private TextMeshProUGUI tooltipText;
        [SerializeField] private Image statusIcon;
        [SerializeField] private Sprite checkmarkSprite;
        [SerializeField] private Sprite crossSprite;

        public void Setup(Sprite icon, string displayName)
        {
            if (iconImage != null)
                iconImage.sprite = icon;

            if (tooltipText != null)
                tooltipText.text = displayName;
        }

        public void SetCount(int have, int need)
        {
            if (countText != null)
            {
                var hasEnough = have >= need;
                countText.text = $"{have}/{need}";
                countText.color = hasEnough ? Color.white : Color.red;
            }
        }

        public void SetStaticCount(int count)
        {
            if (countText != null)
            {
                countText.text = count.ToString();
                countText.color = Color.white;
            }
        }

        public void SetAvailability(bool hasEnough)
        {
            if (statusIcon == null)
                return;

            statusIcon.sprite = hasEnough ? checkmarkSprite : crossSprite;
            statusIcon.color = hasEnough ? Color.green : Color.red;
        }
    }
}