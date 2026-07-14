using System;
using UnityEngine;
using Zenject;
using UnityEngine.SceneManagement;

namespace InputModule
{
    [CreateAssetMenu(fileName = "InputInstaller", menuName = "Installers/InputInstaller")]
    public class InputInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<GameInputService>().AsSingle();
            Container.BindInterfacesTo<InputResetHandler>().AsSingle();
        }
    }

    public class InputResetHandler : IInitializable, IDisposable
    {
        private readonly IInputService _inputService;

        public InputResetHandler(IInputService inputService)
        {
            _inputService = inputService;
        }

        public void Initialize()
        {
            _inputService.Reset();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _inputService.Reset();
        }

        public void Dispose()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            _inputService.Disable();
        }
    }
}