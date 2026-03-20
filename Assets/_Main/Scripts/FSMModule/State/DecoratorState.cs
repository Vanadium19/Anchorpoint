using System;

namespace FSMModule
{
    public class DecoratorState : IState
    {
        private readonly IState _state;

        private readonly Action _onEnter;
        private readonly Action<float> _onUpdate;
        private readonly Action _onExit;

        public DecoratorState(IState state,
            Action onEnter = null,
            Action<float> onUpdate = null,
            Action onExit = null)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));

            _onEnter = onEnter;
            _onUpdate = onUpdate;
            _onExit = onExit;
        }

        public void OnEnter()
        {
            _onEnter?.Invoke();
            _state.OnEnter();
        }

        public void OnUpdate(float deltaTime)
        {
            _onUpdate?.Invoke(deltaTime);
            _state.OnUpdate(deltaTime);
        }

        public void OnExit()
        {
            _onExit?.Invoke();
            _state.OnExit();
        }
    }
}