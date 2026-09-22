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

        private GameObject _panelRoot;

        public event Action UpgradeRequested;
        public event Action CloseRequested;

        private void OnDestroy()
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

            EnsureControls();

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

        public void Show()
        {
            EnsureControls();
            _panelRoot.SetActive(true);
        }

        public void Hide()
        {
            if (_panelRoot != null)
                _panelRoot.SetActive(false);
        }

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

        private void EnsureControls()
        {
            if (buildingNameText != null && levelText != null && priceText != null
                && upgradeButton != null && upgradeButtonText != null && closeButton != null)
                return;

            _panelRoot = new GameObject(
                "BuildingUpgradeUI",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));
            var canvas = _panelRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            var panelObject = new GameObject("UpgradeContent", typeof(RectTransform), typeof(Image));
            var panelTransform = panelObject.GetComponent<RectTransform>();
            panelTransform.SetParent(_panelRoot.transform, false);
            panelTransform.anchorMin = new Vector2(0.5f, 0.5f);
            panelTransform.anchorMax = new Vector2(0.5f, 0.5f);
            panelTransform.sizeDelta = new Vector2(420f, 260f);

            var panelImage = panelObject.GetComponent<Image>();
            panelImage.color = new Color(0.08f, 0.08f, 0.08f, 0.95f);

            buildingNameText = CreateText(panelTransform, "BuildingName", new Vector2(0f, 90f), new Vector2(360f, 40f), 28f);
            levelText = CreateText(panelTransform, "Level", new Vector2(0f, 45f), new Vector2(360f, 32f), 20f);
            priceText = CreateText(panelTransform, "Price", new Vector2(0f, -10f), new Vector2(360f, 70f), 18f);
            upgradeButton = CreateButton(panelTransform, "UpgradeButton", new Vector2(0f, -90f), new Vector2(180f, 42f), out upgradeButtonText);
            closeButton = CreateButton(panelTransform, "CloseButton", new Vector2(180f, 100f), new Vector2(36f, 36f), out var closeButtonText);
            closeButtonText.text = "×";
            upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
            closeButton.onClick.AddListener(OnCloseButtonClicked);
            _panelRoot.SetActive(false);
        }

        private static TextMeshProUGUI CreateText(
            Transform parent,
            string objectName,
            Vector2 position,
            Vector2 size,
            float fontSize)
        {
            var textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
            var textTransform = textObject.GetComponent<RectTransform>();
            textTransform.SetParent(parent, false);
            textTransform.anchoredPosition = position;
            textTransform.sizeDelta = size;

            var text = textObject.GetComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            return text;
        }

        private static Button CreateButton(
            Transform parent,
            string objectName,
            Vector2 position,
            Vector2 size,
            out TextMeshProUGUI text)
        {
            var buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
            var buttonTransform = buttonObject.GetComponent<RectTransform>();
            buttonTransform.SetParent(parent, false);
            buttonTransform.anchoredPosition = position;
            buttonTransform.sizeDelta = size;

            var image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.25f, 0.25f, 0.25f, 1f);

            var button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;
            text = CreateText(buttonTransform, "Label", Vector2.zero, size, 18f);
            return button;
        }
    }
}
