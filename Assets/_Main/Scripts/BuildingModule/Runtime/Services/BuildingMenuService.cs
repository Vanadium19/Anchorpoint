using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BuildingModule
{
    public class BuildingMenuService : IBuildingMenuService
    {
        private readonly Dictionary<BuildingCategory, BuildingMenuCategory> _categories = new();
        private readonly Stack<BuildingMenuCategory> _categoryStack = new();

        private BuildingMenuCategory _currentCategory;
        private List<BuildingMenuItem> _currentItems = new();
        private int _selectedIndex;

        public void Initialize(BuildingCatalog catalog)
        {
            _categories.Clear();
            _categoryStack.Clear();

            var categoryConfigs = catalog.GetCategoryConfigs();

            var allConfigs = catalog.GetAll();
            var groupedByCategory = allConfigs.GroupBy(c => c.Category);

            foreach (var group in groupedByCategory)
            {
                var category = group.Key;
                var sortedConfigs = group.OrderBy(c => c.Order).ToList();
                var firstConfig = sortedConfigs.FirstOrDefault();
                var categoryOrder = firstConfig?.Order ?? 0;
                var categoryIcon = (Sprite)null;
                var categoryDisplayName = category.ToString();

                var categoryConfig = categoryConfigs?.FirstOrDefault(c => c.Category == category);

                if (categoryConfig != null)
                {
                    categoryOrder = categoryConfig.Order;
                    categoryIcon = categoryConfig.Icon;
                    
                    if (!string.IsNullOrEmpty(categoryConfig.DisplayName))
                        categoryDisplayName = categoryConfig.DisplayName;
                }

                var categoryData = new BuildingMenuCategory(category, categoryDisplayName, categoryIcon, categoryOrder);

                foreach (var config in sortedConfigs)
                {
                    var item = new BuildingMenuItem(config.Id, config.Category, false, config.DisplayName, config.Icon, config.Order);
                    categoryData.Items.Add(item);
                }

                _categories[category] = categoryData;
            }

            RefreshCurrentItems();
        }

        public void EnterCategory()
        {
            if (_selectedIndex >= 0 && _selectedIndex < _currentItems.Count)
            {
                var selectedItem = _currentItems[_selectedIndex];

                if (selectedItem.IsCategory)
                    if (_categories.TryGetValue(selectedItem.Category, out var categoryData))
                    {
                        _categoryStack.Push(_currentCategory);
                        _currentCategory = categoryData;
                        _selectedIndex = 0;
                        RefreshCurrentItems();
                    }
                else
                {
                    _categoryStack.Push(_currentCategory);
                    _currentCategory = null;
                    RefreshCurrentItems();
                }
            }
        }

        public void ExitCategory()
        {
            if (_categoryStack.Count > 0)
            {
                _currentCategory = _categoryStack.Pop();
                _selectedIndex = 0;
                RefreshCurrentItems();
            }
        }

        public void SelectNext()
        {
            if (_currentItems.Count == 0)
                return;

            _selectedIndex = (_selectedIndex + 1) % _currentItems.Count;
        }

        public void SelectPrevious()
        {
            if (_currentItems.Count == 0)
                return;

            _selectedIndex = (_selectedIndex - 1 + _currentItems.Count) % _currentItems.Count;
        }

        public BuildingMenuItem GetSelectedItem()
        {
            if (_selectedIndex >= 0 && _selectedIndex < _currentItems.Count)
                return _currentItems[_selectedIndex];
                
            return null;
        }

        public List<BuildingMenuItem> GetCurrentItems() => _currentItems;

        public bool IsInCategory() => _currentCategory != null;

        private void RefreshCurrentItems()
        {
            _currentItems.Clear();

            if (_currentCategory != null)
                _currentItems = _currentCategory.Items.OrderBy(i => i.Order).ToList();
            else
            {
                _currentItems = _categories.Values
                .OrderBy(c => c.Order)
                .Select(c =>
                {
                    var icon = c.Icon;

                    if (icon == null && c.Items.Count > 0)
                        icon = c.Items[0].Icon;
                        
                    return new BuildingMenuItem(c.Category.ToString(), c.Category, true, c.DisplayName, icon, c.Order);
                })
                .ToList();
            }

            _selectedIndex = 0;
        }
    }
}
