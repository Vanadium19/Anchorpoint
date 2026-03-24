using System;

namespace FSMModule
{
    public class BaseState : IState
    {
        private readonly Action _onEnter;
        private readonly Action<float> _onUpdate;
        private readonly Action _onExit;

        public BaseState(Action onEnter = null, Action<float> onUpdate = null, Action onExit = null)
        {
            _onEnter = onEnter;
            _onUpdate = onUpdate;
            _onExit = onExit;
        }

        public void OnEnter() => _onEnter?.Invoke();

        public void OnUpdate(float deltaTime) => _onUpdate?.Invoke(deltaTime);

        public void OnExit() => _onExit?.Invoke();
    }
}