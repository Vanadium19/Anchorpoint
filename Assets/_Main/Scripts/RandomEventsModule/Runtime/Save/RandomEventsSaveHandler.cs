using System;
using SaveModule;
using Zenject;

namespace RandomEventsModule
{
    /// <summary>
    /// Registers <see cref="RandomEventsSaveable"/> in the shared game save.
    /// Does not call <see cref="IGameSaveLoader.Load"/> — loading is driven by the scene
    /// handlers (<c>CampSaveHandler</c> / <c>GameSceneSaveHandler</c>), so this handler only
    /// has to register before them.
    /// </summary>
    public class RandomEventsSaveHandler : IInitializable, IDisposable
    {
        private readonly IGameSaveLoader _gameSaveLoader;
        private readonly RandomEventsSaveable _saveable;

        /// <summary>Creates the handler for the given saveable.</summary>
        public RandomEventsSaveHandler(
            [Inject(Id = GameSaveLoaderIds.Game)] IGameSaveLoader gameSaveLoader,
            RandomEventsSaveable saveable)
        {
            _gameSaveLoader = gameSaveLoader;
            _saveable = saveable;
        }

        /// <summary>Registers the saveable with the game save loader.</summary>
        public void Initialize()
        {
            _gameSaveLoader.RegisterSaveable(_saveable);
        }

        /// <summary>Unregisters the saveable from the game save loader.</summary>
        public void Dispose()
        {
            _gameSaveLoader.UnregisterSaveable(_saveable);
        }
    }
}
