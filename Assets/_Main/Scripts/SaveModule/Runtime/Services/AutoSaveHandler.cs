using UnityEngine;
using Zenject;

namespace SaveModule
{
    public class AutoSaveHandler : IInitializable, ILateDisposable
    {
        private readonly IGameSaveLoader _gameSaveLoader;

        public AutoSaveHandler(
            [Inject(Id = GameSaveLoaderIds.Game)] IGameSaveLoader gameSaveLoader)
        {
            _gameSaveLoader = gameSaveLoader;
        }

        public void Initialize()
        {
            Application.quitting += OnApplicationQuitting;
        }

        public void LateDispose()
        {
            Application.quitting -= OnApplicationQuitting;
        }

        private void OnApplicationQuitting()
        {
            _gameSaveLoader.Save();
        }
    }
}
