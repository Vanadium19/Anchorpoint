using UnityEngine;

namespace BuildingModule
{
    public class BuildingPricePresenter : IBuildingPricePresenter
    {
        private readonly BuildingPricePanelView _view;

        public BuildingPricePresenter(BuildingPricePanelView view)
        {
            _view = view;
        }

        public void Show(BuildPriceInfo info)
        {
            if (_view == null)
                return;

            _view.gameObject.SetActive(true);
            _view.SetData(info);
        }

        public void Hide()
        {
            if (_view == null)
                return;

            _view.gameObject.SetActive(false);
            _view.Clear();
        }
    }
}
