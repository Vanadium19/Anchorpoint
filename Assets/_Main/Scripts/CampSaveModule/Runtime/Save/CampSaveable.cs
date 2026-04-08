using System.Collections.Generic;
using Newtonsoft.Json;
using SaveModule;
using BaseModule;
using BuildingModule;

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

        public string CreateMemento()
        {
            var memento = new CampMemento
            {
                BasePoints = _baseLevelService.CurrentPoints,
                Buildings = new List<BuildingSnapshot>(_buildingSaveService.GetAllSnapshots())
            };

            return JsonConvert.SerializeObject(memento);
        }

        public void RestoreMemento(string data)
        {
            var memento = JsonConvert.DeserializeObject<CampMemento>(data);

            if (memento == null)
                return;

            _baseLevelService.SetPoints(memento.BasePoints);

            _buildingSaveService.ClearAll();

            foreach (var buildingSnapshot in memento.Buildings)
            {
                _buildingSaveService.RestoreFromSnapshot(buildingSnapshot);
            }
        }
    }
}
