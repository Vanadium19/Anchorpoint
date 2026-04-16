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

        public void SetData(PriceItemInfo info, string amountFormat)
        {
            if (info == null || info.ItemData == null)
                return;

            if (iconImage != null)
                iconImage.sprite = info.ItemData.Icon;

            if (nameText != null)
                nameText.text = info.ItemData.DisplayName;

            if (amountText != null)
                amountText.text = string.Format(amountFormat, info.Available, info.Cost);
        }
    }
}
