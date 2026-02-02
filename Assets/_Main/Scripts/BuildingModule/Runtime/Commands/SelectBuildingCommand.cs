using CommandsModule;
using Cysharp.Threading.Tasks;

namespace BuildingModule.Runtime.Commands
{
    public class SelectBuildingCommand : IAsyncCommand<BuildingName>
    {
        private readonly IPlacementService _placementService;

        public SelectBuildingCommand(IPlacementService placementService)
        {
            _placementService = placementService;
        }

        public UniTask<TaskResult> Execute(BuildingName name)
        {
            _placementService.SetBuilding(name);
            return UniTask.FromResult(TaskResult.Success);
        }
    }
}