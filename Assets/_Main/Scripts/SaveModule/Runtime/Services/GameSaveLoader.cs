using System.Collections.Generic;
using UnityEngine;

namespace SaveModule
{
    public class GameSaveLoader : IGameSaveLoader
    {
        private readonly IGameRepository _repository;
        private readonly string _filePath;
        private readonly List<ISaveable> _saveables = new();
        private readonly Dictionary<string, ISaveable> _saveableMap = new();

        public GameSaveLoader(IGameRepository repository, string filePath)
        {
            _repository = repository;
            _filePath = filePath;
        }

        public void RegisterSaveable(ISaveable saveable)
        {
            if (saveable == null)
                return;

            var key = saveable.SaveKey;

            if (_saveableMap.TryGetValue(key, out var existing))
            {
                if (existing == saveable)
                    return;

                _saveables.Remove(existing);
            }

            _saveableMap[key] = saveable;
            _saveables.Add(saveable);
        }

        public void UnregisterSaveable(ISaveable saveable)
        {
            if (saveable == null)
                return;

            var key = saveable.SaveKey;

            if (_saveableMap.Remove(key))
                _saveables.Remove(saveable);
        }

        public void Save()
        {
            var existingData = _repository.Load<GameSaveData>(_filePath);
            var state = existingData?.State ?? new Dictionary<string, string>();

            foreach (var saveable in _saveables)
            {
                state[saveable.SaveKey] = saveable.CreateMementoJson();
            }

            var saveData = new GameSaveData { State = state };
            _repository.Save(saveData, _filePath);
        }

        public void Load()
        {
            var saveData = _repository.Load<GameSaveData>(_filePath);

            if (saveData?.State == null || saveData.State.Count == 0)
                return;

            foreach (var saveable in _saveables)
            {
                if (saveData.State.TryGetValue(saveable.SaveKey, out var json))
                    saveable.RestoreMementoFromJson(json);
            }
        }
    }
}
