using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InventoryModule
{
    public class InventoryItemView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text amountText;
        [SerializeField] private RectTransform rectTransform;
        public void Setup(InventoryItem item, ItemConfig config, float tileSize, float spacing)
        {
            iconImage.sprite = config.Icon;

            if (item.Amount > 1)
            {
                amountText.gameObject.SetActive(true);
                amountText.text = item.Amount.ToString();
            }
            else
            {
                amountText.gameObject.SetActive(false);
            }
            float width = (config.Width * tileSize) + ((config.Width - 1) * spacing);
            float height = (config.Height * tileSize) + ((config.Height - 1) * spacing);
            rectTransform.sizeDelta = new Vector2(width, height);
            float posX = (item.Position.x * tileSize) + (item.Position.x * spacing);
            float posY = -((item.Position.y * tileSize) + (item.Position.y * spacing));

            rectTransform.anchoredPosition = new Vector2(posX, posY);
        }
    }
}