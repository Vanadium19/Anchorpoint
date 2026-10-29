using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArsenalModule
{
    /// <summary>
    /// Displays a single cost item icon and amount inside a modification row.
    /// </summary>
    /// <remarks>
    /// The amount is tinted with <c>notEnoughColor</c> when the player cannot pay it.
    /// </remarks>
    public class ArsenalCostSlotView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private Color enoughColor = Color.white;
        [SerializeField] private Color notEnoughColor = Color.red;

        /// <summary>
        /// Renders the icon and amount for the given cost.
        /// </summary>
        /// <param name="data">The cost display data.</param>
        public void Render(ArsenalCostDisplayData data)
        {
            if (data == null)
                return;

            if (iconImage != null)
                iconImage.sprite = data.Icon;

            if (countText == null)
                return;

            countText.text = data.Count.ToString();
            countText.color = data.HasEnough ? enoughColor : notEnoughColor;
        }
    }
}
