using FSMModule;
using UtilsModule;
using Zenject;

namespace EnemyModule
{
    public abstract class ZenjectStateAsset<TState> : IStateAsset<GameObjectContext> where TState : IState
    {
        public IState Create(GameObjectContext context) => context.Container.CreateBindAndReturn<TState>();
    }
}