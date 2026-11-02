using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ArsenalModule
{
    /// <summary>
    /// One resource a modification costs: icon, name and an owned/required counter.
    /// </summary>
    public class ArsenalCostRowView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private Color enoughColor = Color.white;
        [SerializeField] private Color missingColor = Color.red;

        /// <summary>
        /// Renders the cost from precomputed display data.
        /// </summary>
        /// <param name="data">The cost's display data, including owned and required counts.</param>
        public void Render(ArsenalCostDisplayData data)
        {
            if (iconImage != null)
            {
                iconImage.sprite = data.Icon;
                iconImage.enabled = data.Icon != null;
            }

            if (nameText != null)
                nameText.text = data.ItemName;

            if (countText != null)
            {
                countText.text = $"{data.Owned}/{data.Required}";
                countText.color = data.IsEnough ? enoughColor : missingColor;
            }
        }
    }
}
