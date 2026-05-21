using UnityEngine;
using Zenject;

namespace SaveModule
{
    public class SaveModuleInstaller : MonoInstaller
    {
        [SerializeField] private string gameSaveFilePath = "Saves/GameSave.json";
        [SerializeField] private string settingsSaveFilePath = "Saves/SettingsSave.json";

        public override void InstallBindings()
        {
            Container.Bind<IGameRepository>()
                .To<GameRepository>()
                .AsSingle();

            Container.Bind<IGameSaveLoader>()
                .WithId(GameSaveLoaderIds.Game)
                .To<GameSaveLoader>()
                .AsCached()
                .WithArguments(gameSaveFilePath);

            Container.Bind<IGameSaveLoader>()
                .WithId(GameSaveLoaderIds.Settings)
                .To<GameSaveLoader>()
                .AsCached()
                .WithArguments(settingsSaveFilePath);

            Container.Bind<AutoSaveHandler>()
                .AsSingle()
                .NonLazy();
        }
    }
}