using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace BaseModule
{
    public class BaseLevelView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform progressBarBG;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI pointsText;
        [SerializeField] private Image progressFill;
        [SerializeField] private Image previewFill;
        [SerializeField] private TextMeshProUGUI previewPointsText;

        private string _previewPointsFormat = "+{0}";

        private Tween _levelPunchTween;
        private Tween _previewPunchTween;
        private Tween _slideTween;

        private Vector2 _originalAnchoredPosition;
        private bool _isInitialized;

        public void SetProgress(float fillAmount) => progressFill.fillAmount = fillAmount;

        public void AnimateProgress(float targetFill, float duration)
        {
            DOTween.To(() => progressFill.fillAmount, x => progressFill.fillAmount = x, targetFill, duration)
                .SetEase(Ease.OutQuad);
        }

        public void AnimatePreviewProgress(float targetFill, float duration)
        {
            DOTween.To(() => previewFill.fillAmount, x => previewFill.fillAmount = x, targetFill, duration)
                .SetEase(Ease.OutQuad);
        }

        public void SetPreviewProgress(float fillAmount) => previewFill.fillAmount = fillAmount;

        public void SetPreviewPointsFormat(string format) => _previewPointsFormat = format;

        public void ShowPreviewPoints(int points)
        {
            previewPointsText.gameObject.SetActive(true);
            previewPointsText.text = string.Format(_previewPointsFormat, points);
        }

        public void HidePreviewPoints()
        {
            previewPointsText.gameObject.SetActive(false);
        }

        public void ShowPreviewFill()
        {
            previewFill.gameObject.SetActive(true);
        }

        public void HidePreviewFill()
        {
            previewFill.gameObject.SetActive(false);
        }

        public void Show() => canvasGroup.alpha = 1f;

        public void Hide() => canvasGroup.alpha = 0f;

        public void AnimateShow(float slideOffset, float duration)
        {
            if (!_isInitialized)
            {
                _originalAnchoredPosition = progressBarBG.anchoredPosition;
                _isInitialized = true;
            }

            _slideTween?.Kill();
            canvasGroup.alpha = 1f;
            progressBarBG.anchoredPosition = _originalAnchoredPosition + Vector2.down * slideOffset;

            _slideTween = progressBarBG
                .DOAnchorPos(_originalAnchoredPosition, duration)
                .SetEase(Ease.OutQuad);
        }

        public void AnimateHide(float slideOffset, float duration)
        {
            if (!_isInitialized)
            {
                _originalAnchoredPosition = progressBarBG.anchoredPosition;
                _isInitialized = true;
            }

            _slideTween?.Kill();

            _slideTween = progressBarBG
                .DOAnchorPos(_originalAnchoredPosition + Vector2.down * slideOffset, duration)
                .SetEase(Ease.InQuad)
                .OnComplete(() => canvasGroup.alpha = 0f);
        }

        public void SetLevelText(string text) => levelText.text = text;

        public void SetPointsText(string text) => pointsText.text = text;

        public void AnimateLevelPunch(float punchScale, float duration, int vibrato, float elasticity)
        {
            _levelPunchTween?.Kill();
            progressBarBG.localScale = Vector3.one;
            
            _levelPunchTween = progressBarBG
                .DOPunchScale(Vector3.one * punchScale, duration, vibrato, elasticity)
                .SetEase(Ease.OutQuad);
        }

        public void AnimatePreviewPunch(float punchScale, float duration, int vibrato, float elasticity)
        {
            _previewPunchTween?.Kill();
            previewPointsText.transform.localScale = Vector3.one;
            
            _previewPunchTween = previewPointsText.transform
                .DOPunchScale(Vector3.one * punchScale, duration, vibrato, elasticity)
                .SetEase(Ease.OutQuad);
        }
    }
}
