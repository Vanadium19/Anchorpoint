using System;
using System.Collections.Generic;
using UnityEngine;
using SaveModule;
using BaseModule;
using BuildingModule;
using Sirenix.Serialization;

namespace CampSaveModule
{
    public class CampSaveable : ISaveable
    {
        private const string Key = "camp";

        private readonly IBaseLevelService _baseLevelService;
        private readonly IBuildingSaveService _buildingSaveService;

        public string SaveKey => Key;

        public CampSaveable(
            IBaseLevelService baseLevelService,
            IBuildingSaveService buildingSaveService)
        {
            _baseLevelService = baseLevelService;
            _buildingSaveService = buildingSaveService;
        }

        public string CreateMementoJson()
        {
            var memento = new CampMemento
            {
                BasePoints = _baseLevelService.CurrentPoints,
                Buildings = new List<BuildingSnapshot>(_buildingSaveService.GetAllSnapshots())
            };

            var bytes = SerializationUtility.SerializeValue(memento, DataFormat.JSON);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        public void RestoreMementoFromJson(string json)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            var memento = SerializationUtility.DeserializeValue<CampMemento>(bytes, DataFormat.JSON);

            if (memento == null)
            {
                Debug.LogWarning("[CampSaveable] Failed to parse memento");
                return;
            }

            _baseLevelService.SetPoints(memento.BasePoints);

            _buildingSaveService.ClearAll();

            foreach (var buildingSnapshot in memento.Buildings)
            {
                _buildingSaveService.RestoreFromSnapshot(buildingSnapshot);
            }
        }
    }
}
