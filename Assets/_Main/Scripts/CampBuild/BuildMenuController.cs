using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
namespace CampBuild
{
    public sealed class BuildMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject buildMenuPanel;
        [SerializeField] private FreeCameraController freeCameraController;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private PlacementManager placementManager;
        [SerializeField] private GameObject buildPrefab;
        [SerializeField] private GridVisual gridVisual;

        private CancellationTokenSource _fadeCts;
        private bool _isPlacing;

        private void OnDisable()
        {
            CancelFade();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ToggleMenu().Forget();
            }
        }

        public void StartPlacing()
        {
            StartPlacingAsync().Forget();
        }

        public void FinishPlacing()
        {
            _isPlacing = false;

            buildMenuPanel.SetActive(true);
            SetMenuAlpha(1f);
            SetMenuInteractable(true);

            placementManager.Reset();
            gridVisual.SetVisible(false);
        }

        public void OnCampFireButtonClicked()
        {
            StartPlacing();
            placementManager.StartPlacement(buildPrefab);
        }

        private async UniTask ToggleMenu()
        {
            if (_isPlacing)
            {
                CancelAll();
                return;
            }

            if (buildMenuPanel.activeSelf)
            {
                CancelAll();
                return;
            }

            buildMenuPanel.SetActive(true);
            SetMenuAlpha(1f);
            SetMenuInteractable(true);

            freeCameraController.SetActive(false);
            await UniTask.CompletedTask;
        }

        private void CancelAll()
        {
            CancelFade();

            buildMenuPanel.SetActive(false);
            SetMenuAlpha(1f);
            SetMenuInteractable(true);

            freeCameraController.SetActive(false);
            placementManager.Reset();
            gridVisual.SetVisible(false);

            _isPlacing = false;
        }

        private async UniTask StartPlacingAsync()
        {
            _isPlacing = true;

            await FadeMenuAsync(visible: false);

            freeCameraController.SetActive(true);
            gridVisual.SetVisible(true);
        }

        private async UniTask FadeMenuAsync(bool visible)
        {
            if (canvasGroup == null)
                return;

            CancelFade();
            _fadeCts = new CancellationTokenSource();
            var ct = _fadeCts.Token;

            if (visible)
                buildMenuPanel.SetActive(true);

            float startAlpha = canvasGroup.alpha;
            float targetAlpha = visible ? 1f : 0.5f;

            SetMenuInteractable(false);

            if (fadeDuration <= 0f || Mathf.Approximately(startAlpha, targetAlpha))
            {
                SetMenuAlpha(targetAlpha);
                SetMenuInteractable(visible);

                if (!visible)
                    buildMenuPanel.SetActive(false);

                return;
            }

            float t = 0f;
            while (t < fadeDuration)
            {
                ct.ThrowIfCancellationRequested();

                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / fadeDuration);
                SetMenuAlpha(Mathf.Lerp(startAlpha, targetAlpha, k));

                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            SetMenuAlpha(targetAlpha);
            SetMenuInteractable(visible);

            if (!visible)
                buildMenuPanel.SetActive(false);
        }

        private void CancelFade()
        {
            if (_fadeCts == null)
                return;

            _fadeCts.Cancel();
            _fadeCts.Dispose();
            _fadeCts = null;
        }

        private void SetMenuAlpha(float alpha)
        {
            canvasGroup.alpha = alpha;
        }

        private void SetMenuInteractable(bool value)
        {
            canvasGroup.interactable = value;
            canvasGroup.blocksRaycasts = value;
        }
    }
}