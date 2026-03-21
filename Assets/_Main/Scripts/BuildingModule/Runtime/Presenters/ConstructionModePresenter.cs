using System;
using UnityEngine;
using Zenject;
using InputModule;

namespace BuildingModule
{
    public class ConstructionModePresenter : IInitializable, IDisposable
    {
        private readonly IConstructionModeService _service;
        private readonly IInputService _inputService;

        private readonly GameObject _constructionPanel;
        private readonly GridView _gridView;
        private readonly bool _useGrid;

        public ConstructionModePresenter(
            IConstructionModeService service,
            IInputService inputService,
            GameObject constructionPanel,
            GridView gridView,
            bool useGrid = true)
        {
            _service = service;
            _inputService = inputService;
            _constructionPanel = constructionPanel;
            _gridView = gridView;
            _useGrid = useGrid;
        }

        public void Initialize() => _service.ActiveChanged += OnActiveChanged;

        public void Dispose() => _service.ActiveChanged -= OnActiveChanged;

        private void OnActiveChanged(bool isActive)
        {
            _gridView.gameObject.SetActive(isActive && _useGrid);
            _constructionPanel.SetActive(isActive);
            _inputService.SetBuildMode(isActive);
        }
    }
}