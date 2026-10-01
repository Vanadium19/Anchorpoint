using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UtilsModule;

namespace BuildingModule
{
    public class BuildingUpgradeHudView : MonoBehaviour
    {
        private const string HintKey = "building_upgrade_button";
        private const string InsufficientKey = "building_upgrade_insufficient";
        private const string MaximumLevelKey = "building_upgrade_maximum_level";
        private const string FreeKey = "building_upgrade_free";
        private const string AmountFormatKey = "price_amount_format";
        private const string LevelFormatKey = "building_upgrade_level_format";
        private const string CurrentLevelFormatKey = "level_format";
        private const string HealthFormatKey = "building_upgrade_health_format";
        private const string CostKey = "building_upgrade_cost";
        private const float DeniedPunchOffset = 12f;
        private const float DeniedPunchDuration = 0.3f;
        private const int DeniedPunchVibrato = 10;
        private const float UpgradedPunchScale = 0.1f;
        private const float UpgradedPunchDuration = 0.35f;
        private const int UpgradedPunchVibrato = 6;
        private const float PunchElasticity = 0.6f;

        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text titleText;
        [FormerlySerializedAs("priceText")]
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text statsText;
        [SerializeField] private TMP_Text costLabelText;
        [SerializeField] private TMP_Text hintText;
        [SerializeField] private GameObject keyPrompt;
        [SerializeField] private PriceItemView priceItemPrefab;
        [SerializeField] private UnityEngine.UI.Slider progressSlider;

        [Header("Colors")]
        [SerializeField] private Color hintColor = Color.white;
        [SerializeField] private Color insufficientColor = new(1f, 0.35f, 0.3f);
        [SerializeField] private Color maximumLevelColor = Color.white;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioSource holdAudioSource;
        [SerializeField] private AudioClip holdClip;
        [SerializeField] private AudioClip upgradedClip;
        [SerializeField] private AudioClip deniedClip;

        private readonly List<PriceItemView> _priceItems = new();
        private readonly StringBuilder _stats = new();

        private Tween _deniedTween;
        private Tween _upgradedTween;

        private void OnDestroy()
        {
            _deniedTween?.Kill();
            _upgradedTween?.Kill();
        }

        public void Render(BuildingUpgradeInfo info)
        {
            titleText.text = info.BuildingName;

            if (info.CanUpgrade)
            {
                levelText.text = LocalizedText.GetFormatted(LevelFormatKey, info.CurrentLevel, info.NextLevel);
                RenderStats(info);
                RenderHint(info.CanAfford ? HintKey : InsufficientKey, info.CanAfford ? hintColor : insufficientColor);
                RenderPriceItems(info.PriceItems);
                costLabelText.text = LocalizedText.Get(CostKey);
                costLabelText.gameObject.SetActive(info.PriceItems is { Count: > 0 });
                keyPrompt.SetActive(info.CanAfford);
            }
            else
            {
                levelText.text = LocalizedText.GetFormatted(CurrentLevelFormatKey, info.CurrentLevel);
                statsText.gameObject.SetActive(false);
                RenderHint(MaximumLevelKey, maximumLevelColor);
                RenderPriceItems(null);
                costLabelText.gameObject.SetActive(false);
                keyPrompt.SetActive(false);
            }

            progressSlider.gameObject.SetActive(info.CanUpgrade && info.CanAfford);
        }

        public void Show() => canvasGroup.alpha = 1f;

        public void Hide()
        {
            canvasGroup.alpha = 0f;
            progressSlider.value = 0f;
            StopHold();
        }

        public void SetProgress(float progress) => progressSlider.value = Mathf.Clamp01(progress);

        public void PlayHold()
        {
            if (holdAudioSource == null || holdClip == null)
                return;

            holdAudioSource.clip = holdClip;
            holdAudioSource.Play();
        }

        public void StopHold()
        {
            if (holdAudioSource != null)
                holdAudioSource.Stop();
        }

        public void PlayDenied()
        {
            PlayClip(deniedClip);
            _deniedTween?.Kill(true);
            _deniedTween = ((RectTransform)transform)
                .DOPunchAnchorPos(Vector2.right * DeniedPunchOffset, DeniedPunchDuration, DeniedPunchVibrato, PunchElasticity)
                .SetLink(gameObject);
        }

        public void PlayUpgraded()
        {
            PlayClip(upgradedClip);
            _upgradedTween?.Kill(true);
            _upgradedTween = transform
                .DOPunchScale(Vector3.one * UpgradedPunchScale, UpgradedPunchDuration, UpgradedPunchVibrato, PunchElasticity)
                .SetEase(Ease.OutQuad)
                .SetLink(gameObject);
        }

        private void RenderStats(BuildingUpgradeInfo info)
        {
            _stats.Clear();
            var currentHealth = Mathf.RoundToInt(info.CurrentMaxHealth);
            var nextHealth = Mathf.RoundToInt(info.NextMaxHealth);

            if (nextHealth > currentHealth)
                AppendStat(LocalizedText.GetFormatted(HealthFormatKey, currentHealth, nextHealth));

            foreach (var description in info.NextEffectDescriptions)
                AppendStat(description);

            if (info.PriceItems == null || info.PriceItems.Count == 0)
                AppendStat(LocalizedText.Get(FreeKey));

            statsText.text = _stats.ToString();
            statsText.gameObject.SetActive(_stats.Length > 0);
        }

        private void AppendStat(string stat)
        {
            if (_stats.Length > 0)
                _stats.AppendLine();

            _stats.Append(stat);
        }

        private void RenderHint(string key, Color color)
        {
            hintText.text = LocalizedText.Get(key);
            hintText.color = color;
        }

        private void PlayClip(AudioClip clip)
        {
            if (audioSource != null && clip != null)
                audioSource.PlayOneShot(clip);
        }

        private void RenderPriceItems(IReadOnlyList<PriceItemInfo> priceItems)
        {
            if (priceItemPrefab == null)
                return;

            var amountFormat = LocalizedText.Get(AmountFormatKey);
            var itemIndex = 0;

            if (priceItems != null)
            {
                foreach (var priceItem in priceItems)
                {
                    if (priceItem?.ItemData == null)
                        continue;

                    if (itemIndex == _priceItems.Count)
                    {
                        var newItem = Instantiate(priceItemPrefab, progressSlider.transform.parent);
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
