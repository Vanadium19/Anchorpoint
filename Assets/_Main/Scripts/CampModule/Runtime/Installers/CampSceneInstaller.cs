using PlayerModule;
using UnityEngine;
using Zenject;

namespace CampModule
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