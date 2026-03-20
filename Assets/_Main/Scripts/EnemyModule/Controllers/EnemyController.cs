using System;
using Zenject;

namespace EnemyModule
{
    public sealed class EnemyController : IInitializable, ITickable, IDisposable
    {
        private readonly EnemyAIStateMachine _stateMachine;

        public EnemyController(EnemyAIStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public void Initialize() => _stateMachine.Initialize();

        public void Tick() => _stateMachine.Tick();

        public void Dispose() => _stateMachine.Dispose();
    }
}