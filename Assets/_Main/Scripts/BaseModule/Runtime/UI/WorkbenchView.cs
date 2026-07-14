using System.Collections.Generic;
using InventoryModule;
using UnityEngine;
using UnityEngine.UI;

namespace BaseModule
{
    public class WorkbenchView : MonoBehaviour
    {
        [Header("Zone 1 - Recipe List")]
        [SerializeField] private Transform entriesContainer;
        [SerializeField] private WorkbenchEntryView entryPrefab;
        [SerializeField] private Toggle showAvailableToggle;

        [Header("Zone 2 - Control Panel")]
        [SerializeField] private WorkbenchControlPanel controlPanel;

        [Header("Zone 3 - Queue")]
        [SerializeField] private WorkbenchQueueView queueView;

        private ICraftService _craftService;
        private IInventoryManager _inventoryManager;
        private IReadOnlyList<RecipeConfig> _recipes;
        private readonly List<WorkbenchEntryView> _entries = new();
        private bool _showAvailableOnly;

        public void Initialize(
            ICraftService craftService,
            IInventoryManager inventoryManager,
            IReadOnlyList<RecipeConfig> recipes)
        {
            _craftService = craftService;
            _inventoryManager = inventoryManager;
            _recipes = recipes;

            if (showAvailableToggle != null)
            {
                showAvailableToggle.onValueChanged.AddListener(OnFilterChanged);
                _showAvailableOnly = showAvailableToggle.isOn;
            }

            if (controlPanel != null)
            {
                controlPanel.Initialize(OnCraftRequested);
                controlPanel.QuantityChanged += OnQuantityChanged;
            }

            if (queueView != null)
                queueView.Initialize(craftService, inventoryManager);

            _craftService.StateChanged += OnStateChanged;

            OnStateChanged();
        }

        private void OnDestroy()
        {
            if (_craftService != null)
                _craftService.StateChanged -= OnStateChanged;

            if (controlPanel != null)
                controlPanel.QuantityChanged -= OnQuantityChanged;
        }

        public void RefreshFromService()
        {
            OnStateChanged();
        }

        private void OnStateChanged()
        {
            RefreshRecipeList();
            RefreshControlPanel();
            RefreshQueue();
        }

        private void OnQuantityChanged(int quantity)
        {
            RefreshRecipeList();
        }

        private void OnFilterChanged(bool showAvailable)
        {
            _showAvailableOnly = showAvailable;
            RefreshRecipeList();
        }

        private void OnCraftRequested(int count)
        {
            var recipe = _craftService.SelectedRecipe;

            if (recipe == null)
                return;

            _craftService.StartBatch(recipe, count);
        }

        private void RefreshRecipeList()
        {
            if (_entries.Count == 0 && _recipes != null)
            {
                foreach (var recipe in _recipes)
                    CreateEntry(recipe);
            }

            var selected = _craftService.SelectedRecipe;

            if (selected == null && _entries.Count > 0)
                _craftService.SelectRecipe(_entries[0].Recipe);

            var selectedRecipe = _craftService.SelectedRecipe;
            var craftCount = controlPanel != null ? controlPanel.CurrentQuantity : 1;

            foreach (var entry in _entries)
            {
                var isAvailable = _craftService.GetMaxCraftable(entry.Recipe) > 0;
                var show = !_showAvailableOnly || isAvailable;
                entry.gameObject.SetActive(show);

                var isSelected = entry.Recipe == selectedRecipe;
                entry.SetSelected(isSelected);

                if (isSelected)
                    entry.Refresh(craftCount);
                else
                    entry.Refresh(0);
            }

            RefreshCraftButton();
        }

        private void RefreshControlPanel()
        {
            var selected = _craftService.SelectedRecipe;

            if (controlPanel == null || selected == null)
                return;

            var max = _craftService.GetMaxCraftable(selected);
            controlPanel.SetMaxQuantity(max);
            RefreshCraftButton();
        }

        private void RefreshQueue()
        {
            if (queueView != null)
                queueView.Refresh();
        }

        private void RefreshCraftButton()
        {
            if (controlPanel == null) return;

            var recipe = _craftService.SelectedRecipe;

            if (recipe == null)
            {
                controlPanel.SetCraftButtonInteractable(false);
                return;
            }

            var quantity = controlPanel.CurrentQuantity;
            var canCraft = _craftService.GetMaxCraftable(recipe) >= quantity;
            controlPanel.SetCraftButtonInteractable(canCraft);
        }

        private void CreateEntry(RecipeConfig recipe)
        {
            if (entryPrefab == null || entriesContainer == null)
                return;

            var entry = Instantiate(entryPrefab, entriesContainer);
            entry.Initialize(recipe, _craftService, _inventoryManager);
            _entries.Add(entry);
        }
    }
}
