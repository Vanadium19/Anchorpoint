using System.Linq;
using InventoryModule;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BaseModule
{
    public class WorkbenchQueueEntry : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI resultCountText;
        [SerializeField] private TextMeshProUGUI batchCountText;
        [SerializeField] private TextMeshProUGUI extraResultsText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Button actionButton;
        [SerializeField] private Image actionIcon;

        [SerializeField] private Sprite cancelSprite;
        [SerializeField] private Sprite claimSprite;

        private CraftBatch _batch;
        private ReadyCraft _ready;
        private ICraftService _craftService;
        private IInventoryManager _inventoryManager;

        public void SetupBatch(CraftBatch batch, ICraftService craftService)
        {
            _batch = batch;
            _ready = null;
            _craftService = craftService;

            if (iconImage != null)
            {
                iconImage.sprite = batch.Recipe.Results.FirstOrDefault()?.Item?.Icon;
                iconImage.preserveAspect = true;
            }

            var firstResult = batch.Recipe.Results.FirstOrDefault();

            if (resultCountText != null)
                resultCountText.text = firstResult != null && firstResult.Count > 1 ? firstResult.Count.ToString() : "";

            if (batchCountText != null)
                batchCountText.text = $"x{batch.RemainingToCraft}";

            if (extraResultsText != null)
            {
                var resultCount = batch.Recipe.Results.Count;
                extraResultsText.text = resultCount > 1 ? $"+{resultCount - 1}" : "";
            }

            if (actionIcon != null)
                actionIcon.sprite = cancelSprite;

            if (timerText != null)
            {
                var totalRemaining = (batch.RemainingToCraft - 1) * batch.Recipe.CraftTime + batch.Recipe.CraftTime;
                timerText.text = FormatTime(totalRemaining);
            }

            if (actionButton != null)
            {
                actionButton.onClick.RemoveAllListeners();
                actionButton.onClick.AddListener(() => _craftService.CancelBatch(batch.BatchId));
            }
        }

        public void SetupReady(ReadyCraft ready, ICraftService craftService, IInventoryManager inventoryManager)
        {
            _ready = ready;
            _batch = null;
            _craftService = craftService;
            _inventoryManager = inventoryManager;

            if (iconImage != null)
            {
                iconImage.sprite = ready.Recipe.Results.FirstOrDefault()?.Item?.Icon;
                iconImage.preserveAspect = true;
            }

            var firstResult = ready.Recipe.Results.FirstOrDefault();

            if (resultCountText != null)
                resultCountText.text = firstResult != null && firstResult.Count > 1 ? firstResult.Count.ToString() : "";

            if (batchCountText != null && ready.Count > 1)
                batchCountText.text = $"x{ready.Count}";
            else if (batchCountText != null)
                batchCountText.text = "";

            if (extraResultsText != null)
            {
                var resultCount = ready.Recipe.Results.Count;
                extraResultsText.text = resultCount > 1 ? $"+{resultCount - 1}" : "";
            }

            if (timerText != null)
                timerText.text = "";

            if (iconImage != null)
                iconImage.preserveAspect = true;

            if (actionIcon != null)
                actionIcon.sprite = claimSprite;

            var canClaim = CanClaim(ready);

            if (actionButton != null)
            {
                actionButton.interactable = canClaim;
                actionButton.onClick.RemoveAllListeners();
                actionButton.onClick.AddListener(() => _craftService.ClaimReady(ready.ReadyId));
            }
        }

        private void Update()
        {
            if (_batch != null && _craftService != null)
            {
                if (timerText != null)
                {
                    var totalRemaining = (_batch.RemainingToCraft - 1) * _batch.Recipe.CraftTime
                        + Mathf.Max(0f, _batch.Recipe.CraftTime - _batch.CurrentCraftTime);

                    timerText.text = FormatTime(totalRemaining);
                }
            }

            if (_ready != null && actionButton != null && _inventoryManager != null)
                actionButton.interactable = CanClaim(_ready);
        }

        private static string FormatTime(float totalSeconds)
        {
            var ts = System.TimeSpan.FromSeconds(totalSeconds);
            return $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        }

        private bool CanClaim(ReadyCraft ready)
        {
            foreach (var result in ready.Recipe.Results)
            {
                if (_inventoryManager.GetItemCount(result.Item) + result.Count > result.Item.MaxStackSize)
                {
                    var grid = _inventoryManager.MainGrid;
                    var canFit = grid.FindSpaceForObject(new ItemTable(result.Item)) != null
                        || _inventoryManager.GetItemCount(result.Item) > 0;

                    if (!canFit)
                        return false;
                }
            }

            return true;
        }
    }
}