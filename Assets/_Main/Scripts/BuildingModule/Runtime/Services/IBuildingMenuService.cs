using System.Collections.Generic;

namespace BuildingModule
{
    public interface IBuildingMenuService
    {
        void Initialize(BuildingCatalog catalog);
        void EnterCategory();
        void ExitCategory();
        void SelectNext();
        void SelectPrevious();
        BuildingMenuItem GetSelectedItem();
        List<BuildingMenuItem> GetCurrentItems();
        bool IsInCategory();
    }
}
