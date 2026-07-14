using System.Collections.Generic;
using InventoryModule;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BaseModule
{
    public class WorkbenchEntryView : MonoBehaviour
    {
        [Header("Ingredients")]
        [SerializeField] private Transform ingredientsContainer;
        [SerializeField] private ResourceSlotView ingredientSlotPrefab;

        [Header("Arrow")]
        [SerializeField] private TextMeshProUGUI arrowTimeText;

        [Header("Result")]
        [SerializeField] private Transform resultsContainer;
        [SerializeField] private ResourceSlotView resultSlotPrefab;

        [Header("Info")]
        [SerializeField] private TextMeshProUGUI batchCountText;
        [SerializeField] private Button selectButton;

        private RecipeConfig _recipe;
        private ICraftService _craftService;
        private IInventoryManager _inventoryManager;
        private readonly List<ResourceSlotView> _ingredientSlots = new();
        private readonly List<ResourceSlotView> _resultSlots = new();

        public RecipeConfig Recipe => _recipe;

        public void Initialize(
            RecipeConfig recipe,
            ICraftService craftService,
            IInventoryManager inventoryManager)
        {
            _recipe = recipe;
            _craftService = craftService;
            _inventoryManager = inventoryManager;

            CreateIngredientSlots();
            CreateResultSlots();

            if (selectButton != null)
                selectButton.onClick.AddListener(OnSelect);
        }

        public void SetSelected(bool isSelected)
        {
            if (selectButton != null && selectButton.targetGraphic != null)
                selectButton.targetGraphic.color = isSelected
                    ? new Color(0.8f, 0.9f, 1f)
                    : Color.white;
        }

        public void Refresh(int currentCount)
        {
            var displayCount = currentCount > 0 ? currentCount : 1;
            RefreshIngredientSlots(displayCount);
            RefreshArrow(displayCount);
            RefreshBatchCount(displayCount);
        }

        private void OnSelect()
        {
            _craftService.SelectRecipe(_recipe);
        }

        private void CreateIngredientSlots()
        {
            if (ingredientsContainer == null || ingredientSlotPrefab == null)
                return;

            foreach (var ingredient in _recipe.Ingredients)
            {
                var slot = Instantiate(ingredientSlotPrefab, ingredientsContainer);
                slot.Setup(ingredient.Item.Icon, ingredient.Item.DisplayName);
                _ingredientSlots.Add(slot);
            }
        }

        private void CreateResultSlots()
        {
            if (resultsContainer == null || resultSlotPrefab == null)
                return;

            foreach (var result in _recipe.Results)
            {
                var slot = Instantiate(resultSlotPrefab, resultsContainer);
                slot.Setup(result.Item.Icon, result.Item.DisplayName);
                slot.SetStaticCount(result.Count);
                _resultSlots.Add(slot);
            }
        }

        private void RefreshIngredientSlots(int quantity)
        {
            for (int i = 0; i < _ingredientSlots.Count && i < _recipe.Ingredients.Count; i++)
            {
                var ingredient = _recipe.Ingredients[i];
                var have = _inventoryManager.GetItemCount(ingredient.Item);
                var need = ingredient.Count * quantity;
                _ingredientSlots[i].SetCount(have, need);
                _ingredientSlots[i].SetAvailability(have >= need);
            }
        }

        private void RefreshArrow(int count)
        {
            if (arrowTimeText == null)
                return;

            var totalTime = _recipe.CraftTime * count;
            arrowTimeText.text = FormatTime(totalTime);
        }

        private void RefreshBatchCount(int count)
        {
            if (batchCountText == null)
                return;

            if (count > 1)
                batchCountText.text = $"x{count}";
            else
                batchCountText.text = "";
        }
        private static string FormatTime(float totalSeconds)
        {
            var ts = System.TimeSpan.FromSeconds(totalSeconds);
            return $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        }
    }
}