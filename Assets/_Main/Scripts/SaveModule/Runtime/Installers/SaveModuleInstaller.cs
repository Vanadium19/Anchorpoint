using System.IO;
using UnityEngine;
using Zenject;

namespace SaveModule
{
    public class SaveModuleInstaller : MonoInstaller
    {
        [SerializeField] private string gameSaveFileName = "GameSave.json";
        [SerializeField] private string settingsSaveFileName = "SettingsSave.json";

        public override void InstallBindings()
        {
            Container.Bind<IGameRepository>()
                .To<GameRepository>()
                .AsSingle();

            Container.Bind<IGameSaveLoader>()
                .WithId(GameSaveLoaderIds.Game)
                .To<GameSaveLoader>()
                .AsCached()
                .WithArguments(GetSaveFilePath(gameSaveFileName));

            Container.Bind<IGameSaveLoader>()
                .WithId(GameSaveLoaderIds.Settings)
                .To<GameSaveLoader>()
                .AsCached()
                .WithArguments(GetSaveFilePath(settingsSaveFileName));

            Container.Bind<StarterSaveService>()
                .AsSingle()
                .WithArguments(GetSaveFilePath(gameSaveFileName));

            Container.Bind<AutoSaveHandler>()
                .AsSingle()
                .NonLazy();
        }

        private static string GetSaveFilePath(string fileName)
        {
            return Path.Combine(Application.persistentDataPath, "Saves", fileName);
        }
    }
}