using ComponentsModule;
using FSMModule;
using UnityEngine;
using Zenject;

namespace EnemyModule
{
    public sealed class AIAgent : MonoBehaviour
    {
        [SerializeField] private GameObjectContext context;

        [SerializeReference] private AutoStateMachineAsset stateMachineAsset;

        private IStateMachine<StateName> _stateMachine;
        private IHealthComponent _health;

        [Inject]
        public void Construct(IHealthComponent health)
        {
            _health = health;
        }

        private void OnEnable() => _stateMachine?.OnEnter();

        private void Start()
        {
            _stateMachine = (IStateMachine<StateName>)stateMachineAsset.Create(context);
            _stateMachine.OnEnter();
        }

        private void Update()
        {
            if (!_health.IsAlive)
                return;

            _stateMachine?.OnUpdate(Time.deltaTime);
        }

        private void OnDisable() => _stateMachine?.OnExit();
    }
}