using UnityEngine;
using Zenject;

namespace SaveModule
{
    public class SaveModuleInstaller : MonoInstaller
    {
        [SerializeField] private string saveFilePath = "saves/camp.json";

        public override void InstallBindings()
        {
            Container.Bind<IGameRepository>()
                .To<GameRepository>()
                .AsSingle();

            Container.Bind<IGameSaveLoader>()
                .To<GameSaveLoader>()
                .AsSingle()
                .WithArguments(saveFilePath);

            Container.Bind<AutoSaveHandler>()
                .AsSingle()
                .NonLazy();
        }
    }
}
