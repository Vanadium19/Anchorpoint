using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilsModule;

namespace WeaponModule
{
    public class AmmoView : MonoBehaviour
    {
        private const float ColorSmoothSpeed = 10f;
        private const float WearSnapThreshold = 0.0005f;
        private const float PulseCenter = 0.5f;

        [SerializeField] private TMP_Text ammoText;

        [Header("Wear")]
        [SerializeField] private Image wearFill;
        [Tooltip("Wear percent from which the indicator turns yellow.")]
        [Range(0f, 100f)]
        [SerializeField] private float mediumWearThreshold = 30f;
        [Tooltip("Wear percent from which the indicator turns red.")]
        [Range(0f, 100f)]
        [SerializeField] private float highWearThreshold = 70f;
        [SerializeField] private Color lowWearColor = new(0.36f, 0.62f, 0.66f);
        [SerializeField] private Color mediumWearColor = new(0.78f, 0.68f, 0.36f);
        [SerializeField] private Color highWearColor = new(0.66f, 0.36f, 0.62f);
        [Tooltip("Seconds the wear bar takes to catch up with the real wear value.")]
        [Min(0f)]
        [SerializeField] private float wearSmoothTime = 0.15f;
        [Tooltip("Scale of the wear bar at the moment it crosses a color threshold.")]
        [Min(1f)]
        [SerializeField] private float thresholdPunchScale = 1.25f;
        [Min(0.01f)]
        [SerializeField] private float thresholdPunchDuration = 0.35f;

        [Header("Malfunction")]
        [SerializeField] private TMP_Text malfunctionText;
        [SerializeField] private string misfireHintKey = "weapon_misfire_hint";
        [SerializeField] private string jamHintKey = "weapon_jam_hint";
        [Min(0.01f)]
        [SerializeField] private float hintFadeDuration = 0.2f;
        [Tooltip("Scale of the hint at the moment it appears.")]
        [Min(1f)]
        [SerializeField] private float hintPopScale = 1.3f;
        [Tooltip("Pulses per second while the malfunction is active.")]
        [Min(0f)]
        [SerializeField] private float hintPulseSpeed = 1.5f;
        [Range(0f, 1f)]
        [SerializeField] private float hintPulseMinAlpha = 0.55f;

        private float _targetWear;
        private float _displayedWear;
        private float _wearVelocity;
        private Color _displayedColor;
        private int _wearLevel;
        private float _punchProgress = 1f;
        private RectTransform _wearBar;
        private bool _isHintVisible;
        private float _hintProgress;
        private float _hintPulseTime;

        private void Awake()
        {
            _displayedColor = lowWearColor;

            if (wearFill != null)
                _wearBar = wearFill.transform.parent as RectTransform;
        }

        private void Update()
        {
            var deltaTime = Time.unscaledDeltaTime;

            UpdateWear(deltaTime);
            UpdatePunch(deltaTime);
            UpdateHint(deltaTime);
        }

        public void SetAmmo(int currentMagazineAmmo, int reserveAmmo)
        {
            if (ammoText == null)
                return;

            ammoText.text = $"{currentMagazineAmmo} / {reserveAmmo}";
        }

        public void SetWear(float wear, bool isImmediate = false)
        {
            _targetWear = Mathf.Clamp01(wear / WeaponWearModel.MaxWear);

            if (isImmediate)
            {
                _displayedWear = _targetWear;
                _wearVelocity = 0f;
                _wearLevel = GetWearLevel(wear);
                _displayedColor = GetWearColor(_wearLevel);
                _punchProgress = 1f;
                ApplyWear();
                ApplyPunch();
                return;
            }

            var level = GetWearLevel(wear);

            if (level > _wearLevel)
                _punchProgress = 0f;

            _wearLevel = level;
        }

        public void SetMalfunction(WeaponMalfunction malfunction, bool isImmediate = false)
        {
            if (malfunctionText == null)
                return;

            var hintKey = GetHintKey(malfunction);
            var isVisible = !string.IsNullOrEmpty(hintKey);

            if (isVisible)
                malfunctionText.text = LocalizedText.Get(hintKey);

            if (isVisible && !_isHintVisible)
                _hintPulseTime = 0f;

            _isHintVisible = isVisible;

            if (isImmediate)
                _hintProgress = isVisible ? 1f : 0f;

            ApplyHint();
        }

        public void Clear()
        {
            if (ammoText != null)
                ammoText.text = "- / -";

            SetWear(0f, true);
            SetMalfunction(WeaponMalfunction.None, true);
        }

        private void UpdateWear(float deltaTime)
        {
            if (Mathf.Abs(_displayedWear - _targetWear) > WearSnapThreshold)
                _displayedWear = Mathf.SmoothDamp(_displayedWear, _targetWear, ref _wearVelocity, wearSmoothTime,
                    Mathf.Infinity, deltaTime);
            else
                _displayedWear = _targetWear;

            _displayedColor = Color.Lerp(_displayedColor, GetWearColor(_wearLevel), deltaTime * ColorSmoothSpeed);
            ApplyWear();
        }

        private void UpdatePunch(float deltaTime)
        {
            if (_punchProgress >= 1f)
                return;

            _punchProgress = Mathf.Min(1f, _punchProgress + deltaTime / thresholdPunchDuration);
            ApplyPunch();
        }

        private void UpdateHint(float deltaTime)
        {
            var targetProgress = _isHintVisible ? 1f : 0f;

            if (!Mathf.Approximately(_hintProgress, targetProgress))
                _hintProgress = Mathf.MoveTowards(_hintProgress, targetProgress, deltaTime / hintFadeDuration);

            if (_isHintVisible)
                _hintPulseTime += deltaTime;

            ApplyHint();
        }

        private void ApplyWear()
        {
            if (wearFill == null)
                return;

            wearFill.fillAmount = _displayedWear;
            wearFill.color = _displayedColor;
        }

        private void ApplyPunch()
        {
            if (_wearBar == null)
                return;

            var scale = Mathf.Lerp(thresholdPunchScale, 1f, EaseOut(_punchProgress));
            _wearBar.localScale = new Vector3(1f, scale, 1f);
        }

        private void ApplyHint()
        {
            if (malfunctionText == null)
                return;

            var isActive = _hintProgress > 0f;

            if (malfunctionText.gameObject.activeSelf != isActive)
                malfunctionText.gameObject.SetActive(isActive);

            if (!isActive)
                return;

            var pulse = _isHintVisible && hintPulseSpeed > 0f
                ? Mathf.Lerp(hintPulseMinAlpha, 1f, PulseCenter + PulseCenter * Mathf.Sin(_hintPulseTime * hintPulseSpeed * Mathf.PI * 2f))
                : 1f;

            malfunctionText.alpha = _hintProgress * pulse;

            var scale = Mathf.Lerp(hintPopScale, 1f, EaseOut(_hintProgress));
            malfunctionText.rectTransform.localScale = new Vector3(scale, scale, 1f);
        }

        private int GetWearLevel(float wear)
        {
            if (wear >= highWearThreshold)
                return 2;

            return wear >= mediumWearThreshold ? 1 : 0;
        }

        private Color GetWearColor(int level)
        {
            switch (level)
            {
                case 2:
                    return highWearColor;
                case 1:
                    return mediumWearColor;
                default:
                    return lowWearColor;
            }
        }

        private static float EaseOut(float value) => 1f - (1f - value) * (1f - value);

        private string GetHintKey(WeaponMalfunction malfunction)
        {
            switch (malfunction)
            {
                case WeaponMalfunction.Misfire:
                    return misfireHintKey;
                case WeaponMalfunction.Jam:
                    return jamHintKey;
                default:
                    return null;
            }
        }
    }
}
