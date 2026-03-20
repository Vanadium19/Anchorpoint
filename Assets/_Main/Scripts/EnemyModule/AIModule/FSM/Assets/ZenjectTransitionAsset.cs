using FSMModule;
using Zenject;

namespace EnemyModule
{
    public abstract class ZenjectTransitionAsset<TTransition> : IStateTransitionAsset<StateName, GameObjectContext> where TTransition : IStateTransition<StateName>
    {
        public IStateTransition<StateName> Create(GameObjectContext context) => context.Container.Instantiate<TTransition>();
    }
}