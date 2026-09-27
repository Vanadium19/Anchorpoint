using System;
using Zenject;

namespace BuildingModule
{
    public class BuildingUpgradePresenter : IInitializable, IDisposable
    {
        private readonly BuildingUpgradeController _controller;
        private readonly BuildingUpgradeHudView _view;

        private float _progress;

        public BuildingUpgradePresenter(BuildingUpgradeController controller, BuildingUpgradeHudView view)
        {
            _controller = controller;
            _view = view;
        }

        public void Initialize()
        {
            _view.Hide();
            _controller.UpgradeInfoChanged += OnUpgradeInfoChanged;
            _controller.UpgradeProgressChanged += OnUpgradeProgressChanged;
            _controller.UpgradeDenied += OnUpgradeDenied;
            _controller.BuildingUpgraded += OnBuildingUpgraded;
        }

        public void Dispose()
        {
            _controller.UpgradeInfoChanged -= OnUpgradeInfoChanged;
            _controller.UpgradeProgressChanged -= OnUpgradeProgressChanged;
            _controller.UpgradeDenied -= OnUpgradeDenied;
            _controller.BuildingUpgraded -= OnBuildingUpgraded;
        }

        private void OnUpgradeInfoChanged(BuildingUpgradeInfo info)
        {
            if (info == null)
            {
                _view.Hide();
                return;
            }

            _view.Render(info);
            _view.Show();
        }

        private void OnUpgradeProgressChanged(float progress)
        {
            if (_progress <= 0f && progress > 0f)
                _view.PlayHold();
            else if (_progress > 0f && progress <= 0f)
                _view.StopHold();

            _progress = progress;
            _view.SetProgress(progress);
        }

        private void OnUpgradeDenied() => _view.PlayDenied();

        private void OnBuildingUpgraded() => _view.PlayUpgraded();
    }
}
