using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UtilsModule;

namespace BuildingModule
{
    public class BuildingUpgradeHudView : MonoBehaviour
    {
        private const string HintKey = "building_upgrade_button";
        private const string InsufficientKey = "building_upgrade_insufficient";
        private const string FreeKey = "building_upgrade_free";
        private const string ItemFormatKey = "building_upgrade_item_format";
        private const string LevelFormatKey = "building_upgrade_level_format";

        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text hintText;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private PriceItemView priceItemPrefab;
        [SerializeField] private UnityEngine.UI.Slider progressSlider;

        private readonly List<PriceItemView> _priceItems = new();

        public void Render(BuildingUpgradeInfo info)
        {
            hintText.text = LocalizedText.Get(info.CanAfford ? HintKey : InsufficientKey);

            var price = new StringBuilder(info.BuildingName);
            price.Append(" · ");
            price.Append(LocalizedText.GetFormatted(LevelFormatKey, info.CurrentLevel, info.NextLevel));

            if (info.PriceItems == null || info.PriceItems.Count == 0)
            {
                price.AppendLine();
                price.Append(LocalizedText.Get(FreeKey));
            }

            priceText.text = price.ToString();
            RenderPriceItems(info.PriceItems);
        }

        public void Show() => canvasGroup.alpha = 1f;

        public void Hide()
        {
            canvasGroup.alpha = 0f;
            progressSlider.value = 0f;
        }

        public void SetProgress(float progress) => progressSlider.value = Mathf.Clamp01(progress);

        private void RenderPriceItems(IReadOnlyList<PriceItemInfo> priceItems)
        {
            if (priceItemPrefab == null)
                return;

            var amountFormat = LocalizedText.Get(ItemFormatKey);
            var itemIndex = 0;

            if (priceItems != null)
            {
                foreach (var priceItem in priceItems)
                {
                    if (priceItem?.ItemData == null)
                        continue;

                    if (itemIndex == _priceItems.Count)
                    {
                        var newItem = Instantiate(priceItemPrefab, transform);
                        newItem.transform.SetSiblingIndex(progressSlider.transform.GetSiblingIndex());
                        _priceItems.Add(newItem);
                    }

                    var priceItemView = _priceItems[itemIndex++];
                    priceItemView.gameObject.SetActive(true);
                    priceItemView.SetData(priceItem, amountFormat);
                }
            }

            for (; itemIndex < _priceItems.Count; itemIndex++)
                _priceItems[itemIndex].gameObject.SetActive(false);
        }
    }
}
