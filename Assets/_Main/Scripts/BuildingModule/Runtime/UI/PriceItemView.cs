using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BuildingModule
{
    public class PriceItemView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI amountText;
        [SerializeField] private Color insufficientAmountColor = new(1f, 0.35f, 0.3f);

        private Color _sufficientAmountColor;
        private bool _hasSufficientAmountColor;

        public void SetData(PriceItemInfo info, string amountFormat)
        {
            if (info == null || info.ItemData == null)
                return;

            if (iconImage != null)
                iconImage.sprite = info.ItemData.Icon;

            if (nameText != null)
                nameText.text = info.ItemData.DisplayName;

            if (amountText != null)
            {
                if (!_hasSufficientAmountColor)
                {
                    _sufficientAmountColor = amountText.color;
                    _hasSufficientAmountColor = true;
                }

                amountText.text = string.Format(amountFormat, info.Available, info.Cost);
                amountText.color = info.Available >= info.Cost ? _sufficientAmountColor : insufficientAmountColor;
            }
        }
    }
}
