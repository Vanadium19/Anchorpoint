using PlayerModule;
using UnityEngine;
using Zenject;

namespace CampSaveModule
{
    public class CampSceneInstaller : MonoInstaller
    {
        [SerializeField] private PlayerProvider playerProvider;

        public override void InstallBindings()
        {
            Container.Bind<PlayerProvider>()
                .FromInstance(playerProvider)
                .AsSingle();
        }
    }
}