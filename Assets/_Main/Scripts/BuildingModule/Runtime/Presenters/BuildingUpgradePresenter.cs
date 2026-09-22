using System;
using Zenject;

namespace BuildingModule
{
    public class BuildingUpgradePresenter : IInitializable, IDisposable
    {
        private readonly BuildingUpgradeController _controller;
        private readonly BuildingUpgradeHudView _view;

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
        }

        public void Dispose()
        {
            _controller.UpgradeInfoChanged -= OnUpgradeInfoChanged;
            _controller.UpgradeProgressChanged -= OnUpgradeProgressChanged;
            _view.Hide();
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

        private void OnUpgradeProgressChanged(float progress) => _view.SetProgress(progress);
    }
}
