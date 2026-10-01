using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TradeModule
{
    /// <summary>
    /// Displays a single item icon and amount inside a trade offer row.
    /// </summary>
    public class TradeResourceSlotView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI countText;

        /// <summary>
        /// Renders the icon and amount for the given item.
        /// </summary>
        /// <param name="item">The item and amount to display.</param>
        public void Render(TradeItemAmount item)
        {
            if (item == null)
                return;

            if (iconImage != null)
                iconImage.sprite = item.Item != null ? item.Item.Icon : null;

            if (countText != null)
                countText.text = item.Count.ToString();
        }
    }
}
