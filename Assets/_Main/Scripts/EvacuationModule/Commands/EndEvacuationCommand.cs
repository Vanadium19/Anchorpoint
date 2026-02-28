using CommandsModule;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using Zenject;

namespace EvacuationModule
{
    public class EndEvacuationCommand : IAsyncCommand
    {
        [Inject] private EvacuationConfig _config;

        public UniTask<TaskResult> Execute()
        {
            var baseSceneName = _config.BaseSceneName;
            var name = !string.IsNullOrEmpty(baseSceneName) ? baseSceneName : SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(name);
            return UniTask.FromResult(TaskResult.Success);
        }
    }
}