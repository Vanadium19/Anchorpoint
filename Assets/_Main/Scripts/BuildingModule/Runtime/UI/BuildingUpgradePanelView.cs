using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilsModule;

namespace BuildingModule
{
    public class BuildingUpgradePanelView : MonoBehaviour
    {
        private const string ButtonKey = "building_upgrade_button";
        private const string FreeKey = "building_upgrade_free";
        private const string ItemFormatKey = "building_upgrade_item_format";
        private const string LevelFormatKey = "building_upgrade_level_format";
        private const string MaximumLevelKey = "building_upgrade_maximum_level";

        [SerializeField] private TextMeshProUGUI buildingNameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private TextMeshProUGUI upgradeButtonText;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private Button closeButton;

        public event Action UpgradeRequested;
        public event Action CloseRequested;

        private void OnEnable()
        {
            if (upgradeButton != null)
                upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);

            if (closeButton != null)
                closeButton.onClick.AddListener(OnCloseButtonClicked);
        }

        private void OnDisable()
        {
            if (upgradeButton != null)
                upgradeButton.onClick.RemoveListener(OnUpgradeButtonClicked);

            if (closeButton != null)
                closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        }

        public void Render(BuildingUpgradeInfo info)
        {
            if (info == null)
                return;

            if (buildingNameText != null)
                buildingNameText.text = info.BuildingName;

            if (levelText != null)
                levelText.text = info.CanUpgrade
                    ? LocalizedText.GetFormatted(LevelFormatKey, info.CurrentLevel, info.NextLevel)
                    : LocalizedText.GetFormatted(LevelFormatKey, info.CurrentLevel, info.CurrentLevel);

            if (priceText != null)
                priceText.text = FormatPrice(info);

            if (upgradeButton != null)
                upgradeButton.interactable = info.CanUpgrade && info.CanAfford;

            if (upgradeButtonText != null)
                upgradeButtonText.text = LocalizedText.Get(ButtonKey);
        }

        public void Show() => gameObject.SetActive(true);

        public void Hide() => gameObject.SetActive(false);

        private static string FormatPrice(BuildingUpgradeInfo info)
        {
            if (!info.CanUpgrade)
                return LocalizedText.Get(MaximumLevelKey);

            if (info.PriceItems == null || info.PriceItems.Count == 0)
                return LocalizedText.Get(FreeKey);

            return string.Join("\n", info.PriceItems
                .Where(item => item?.ItemData != null)
                .Select(item => LocalizedText.GetFormatted(
                    ItemFormatKey,
                    item.ItemData.DisplayName,
                    item.Available,
                    item.Cost)));
        }

        private void OnUpgradeButtonClicked() => UpgradeRequested?.Invoke();

        private void OnCloseButtonClicked() => CloseRequested?.Invoke();
    }
}
