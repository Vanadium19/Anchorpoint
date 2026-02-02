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

        public ConstructionModePresenter(
            IConstructionModeService service,
            IInputService inputService,
            GameObject constructionPanel,
            GridView gridView)
        {
            _service = service;
            _inputService = inputService;
            _constructionPanel = constructionPanel;
            _gridView = gridView;
        }

        public void Initialize() => _service.ActiveChanged += OnActiveChanged;

        public void Dispose() => _service.ActiveChanged -= OnActiveChanged;

        private void OnActiveChanged(bool isActive)
        {
            _gridView.gameObject.SetActive(isActive);
            _constructionPanel.SetActive(isActive);
            _inputService.SetBuildMode(isActive);
        }
    }
}