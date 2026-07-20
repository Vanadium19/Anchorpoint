using BaseModule;
using ComponentsModule;
using FSMModule;
using UnityEngine;
using Zenject;

namespace EnemyModule
{
    public sealed class AIAgent : MonoBehaviour, IPausable
    {
        [SerializeField] private GameObjectContext context;

        [SerializeReference] private AutoStateMachineAsset stateMachineAsset;

        private IStateMachine<StateName> _stateMachine;
        private IHealthComponent _health;
        private IPathMoveComponent _movement;
        private IPauseManager _pauseManager;
        private bool _isPaused;

        [Inject]
        public void Construct(IHealthComponent health,
            IPathMoveComponent movement,
            IPauseManager pauseManager)
        {
            _health = health;
            _movement = movement;
            _pauseManager = pauseManager;
            _pauseManager.Register(this);
        }

        private void OnEnable() => _stateMachine?.OnEnter();

        private void Start()
        {
            _stateMachine = (IStateMachine<StateName>)stateMachineAsset.Create(context);
            _stateMachine.OnEnter();
        }

        private void Update()
        {
            if (_isPaused || !_health.IsAlive)
                return;

            _stateMachine?.OnUpdate(Time.deltaTime);
        }

        private void OnDisable() => _stateMachine?.OnExit();

        private void OnDestroy() => _pauseManager?.Unregister(this);

        public void SetPaused(bool isPaused)
        {
            _isPaused = isPaused;

            if (isPaused)
                _movement?.Stop();
        }
    }
}
