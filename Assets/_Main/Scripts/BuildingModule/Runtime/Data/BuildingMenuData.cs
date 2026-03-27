using System.Collections.Generic;
using UnityEngine;

namespace BuildingModule
{
    public class BuildingMenuItem
    {
        public string Id { get; }
        public BuildingCategory Category { get; }
        public bool IsCategory { get; }
        public string DisplayName { get; }
        public Sprite Icon { get; }
        public int Order { get; }

        public BuildingMenuItem(string id, BuildingCategory category, bool isCategory, string displayName, Sprite icon, int order = 0)
        {
            Id = id;
            Category = category;
            IsCategory = isCategory;
            DisplayName = displayName;
            Icon = icon;
            Order = order;
        }
    }

    public class BuildingMenuCategory
    {
        public BuildingCategory Category { get; }
        public string DisplayName { get; }
        public Sprite Icon { get; }
        public int Order { get; set; }
        public List<BuildingMenuItem> Items { get; } = new();
        public List<BuildingMenuCategory> SubCategories { get; } = new();

        public BuildingMenuCategory(BuildingCategory category, string displayName, Sprite icon, int order = 0)
        {
            Category = category;
            DisplayName = displayName;
            Icon = icon;
            Order = order;
        }

        public bool IsEmpty => Items.Count == 0 && SubCategories.Count == 0;
    }

    public class BuildingMenuData
    {
        public List<BuildingMenuCategory> Categories { get; } = new();
        public List<BuildingMenuItem> RootItems { get; } = new();

        public bool HasRootItems => RootItems.Count > 0;
    }
}
