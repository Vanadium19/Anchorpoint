using CommandsModule;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using Zenject;
using SaveModule;

namespace EvacuationModule
{
    public class EndEvacuationCommand : IAsyncCommand
    {
        [Inject] private EvacuationConfig _config;
        [Inject(Id = GameSaveLoaderIds.Game)] private IGameSaveLoader _gameSaveLoader;

        public UniTask<TaskResult> Execute()
        {
            _gameSaveLoader.Save();

            var baseSceneName = _config.BaseSceneName;
            var name = !string.IsNullOrEmpty(baseSceneName) ? baseSceneName : SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(name);
            return UniTask.FromResult(TaskResult.Success);
        }
    }
}