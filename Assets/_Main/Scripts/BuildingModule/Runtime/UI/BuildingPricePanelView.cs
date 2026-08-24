using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UtilsModule;

namespace BuildingModule
{
    public class BuildingPricePanelView : MonoBehaviour
    {
        private const string HeaderFormatKey = "price_header_format";
        private const string AmountFormatKey = "price_amount_format";

        [Header("References")]
        [SerializeField] private TextMeshProUGUI headerText;
        [SerializeField] private RectTransform contentContainer;
        [SerializeField] private PriceItemView itemPrefab;

        [Header("Config")]
        [SerializeField] private BuildingPriceUIConfig config;

        private readonly List<PriceItemView> _items = new();

        public void SetData(BuildPriceInfo info)
        {
            if (info == null)
            {
                Clear();
                return;
            }

            UpdateHeader(info);
            UpdateItems(info);
        }

        public void Clear()
        {
            if (headerText != null)
                headerText.text = string.Empty;

            ClearItems();
        }

        private void UpdateHeader(BuildPriceInfo info)
        {
            if (headerText == null)
                return;

            headerText.text = LocalizedText.GetFormatted(HeaderFormatKey, info.BuildingName, info.AvailableCount);
        }

        private void UpdateItems(BuildPriceInfo info)
        {
            ClearItems();

            if (contentContainer == null || itemPrefab == null || config == null)
                return;

            var amountFormat = LocalizedText.Get(AmountFormatKey);

            for (int i = 0; i < info.Items.Count; i++)
            {
                var item = Instantiate(itemPrefab, contentContainer);
                var rectTransform = item.GetComponent<RectTransform>();

                if (rectTransform != null)
                    rectTransform.anchoredPosition = new Vector2(0, -i * config.ItemHeight);

                item.SetData(info.Items[i], amountFormat);
                _items.Add(item);
            }
        }

        private void ClearItems()
        {
            foreach (var item in _items)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }

            _items.Clear();
        }
    }
}
