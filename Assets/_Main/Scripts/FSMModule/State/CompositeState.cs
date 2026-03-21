using System;
using System.Collections.Generic;

namespace FSMModule
{
    public class CompositeState : IState
    {
        private readonly List<IState> _states;

        public CompositeState(List<IState> states)
        {
            _states = states;
        }

        public CompositeState(IEnumerable<IState> states)
        {
            _states = new(states);
        }

        public CompositeState(params IState[] states)
        {
            _states = new(states);
        }

        public void AddState(IState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            _states.Add(state);
        }

        public void RemoveState(IState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            _states.Remove(state);
        }

        public void OnEnter()
        {
            foreach (var state in _states)
                state.OnEnter();
        }

        public void OnUpdate(float deltaTime)
        {
            foreach (var state in _states)
                state.OnUpdate(deltaTime);
        }

        public void OnExit()
        {
            foreach (var state in _states)
                state.OnExit();
        }
    }
}