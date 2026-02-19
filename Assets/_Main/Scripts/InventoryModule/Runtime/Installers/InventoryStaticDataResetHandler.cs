using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace InventoryModule
{
    public class InventoryStaticDataResetHandler : IInitializable, IDisposable
    {
        public void Initialize()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ContainerWindow.ClearAllStaticData();
        }

        public void Dispose()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
