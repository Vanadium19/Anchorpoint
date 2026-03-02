using System;
using UnityEngine.SceneManagement;
using Zenject;

namespace InventoryModule
{
    public class InventoryStaticDataResetHandler : IInitializable, IDisposable
    {
        private readonly IContainerWindowService _windowService;

        public InventoryStaticDataResetHandler(IContainerWindowService windowService)
        {
            _windowService = windowService;
        }

        public void Initialize()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _windowService.Clear();
        }

        public void Dispose()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
