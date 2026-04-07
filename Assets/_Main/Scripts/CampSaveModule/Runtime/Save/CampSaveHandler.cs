using System;
using Zenject;
using SaveModule;

namespace CampSaveModule
{
    public class CampSaveHandler : IInitializable, IDisposable
    {
        private readonly IGameSaveLoader _gameSaveLoader;
        private readonly CampSaveable _campSaveable;

        public CampSaveHandler(IGameSaveLoader gameSaveLoader, CampSaveable campSaveable)
        {
            _gameSaveLoader = gameSaveLoader;
            _campSaveable = campSaveable;
        }

        public void Initialize()
        {
            _gameSaveLoader.RegisterSaveable(_campSaveable);
            _gameSaveLoader.Load();
        }

        public void Dispose()
        {
            _gameSaveLoader.UnregisterSaveable(_campSaveable);
        }
    }
}
