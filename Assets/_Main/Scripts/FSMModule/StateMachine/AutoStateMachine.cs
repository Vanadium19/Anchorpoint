using System;
using System.Collections.Generic;

namespace FSMModule
{
    public class AutoStateMachine<TKey> : IAutoStateMachine<TKey>
    {
        private static readonly EqualityComparer<TKey> _comparer = EqualityComparer<TKey>.Default;

        private readonly IStateMachine<TKey> _stateMachine;
        private readonly List<IStateTransition<TKey>> _transitions;

        public event Action<TKey> OnStateChanged
        {
            add => _stateMachine.OnStateChanged += value;
            remove => _stateMachine.OnStateChanged -= value;
        }

        public event Action<TKey> OnStateAdded
        {
            add => _stateMachine.OnStateAdded += value;
            remove => _stateMachine.OnStateAdded -= value;
        }

        public event Action<TKey> OnStateRemoved
        {
            add => _stateMachine.OnStateRemoved += value;
            remove => _stateMachine.OnStateRemoved -= value;
        }

        public event Action<IStateTransition<TKey>> OnTransitionAdded;
        public event Action<IStateTransition<TKey>> OnTransitionRemoved;

        public AutoStateMachine(TKey initialState,
            IEnumerable<(TKey, IState)> states,
            IEnumerable<IStateTransition<TKey>> transitions)
        {
            _stateMachine = new StateMachine<TKey>(initialState, states);
            _transitions = new(transitions);
        }

        public AutoStateMachine(TKey initialState,
            IEnumerable<KeyValuePair<TKey, IState>> states,
            IEnumerable<IStateTransition<TKey>> transitions)
        {
            _stateMachine = new StateMachine<TKey>(initialState, states);
            _transitions = new(transitions);
        }

        public AutoStateMachine(IStateMachine<TKey> stateMachine, IEnumerable<IStateTransition<TKey>> transitions)
        {
            _stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
            _transitions = new(transitions);
        }

        public AutoStateMachine(IStateMachine<TKey> stateMachine, params IStateTransition<TKey>[] transitions)
        {
            _stateMachine = stateMachine ?? throw new ArgumentNullException(nameof(stateMachine));
            _transitions = new(transitions);
        }

        public int StateCount => _stateMachine.StateCount;
        public TKey CurrentState => _stateMachine.CurrentState;
        public IReadOnlyCollection<TKey> States => _stateMachine.States;

        public int TransitionCount => _transitions.Count;
        public IEnumerable<(TKey, TKey)> Transitions => GetTransitions();

        public void OnEnter() => _stateMachine.OnEnter();

        public void OnUpdate(float deltaTime)
        {
            UpdateTransitions();

            _stateMachine.OnUpdate(deltaTime);
        }

        public void OnExit() => _stateMachine.OnExit();

        public bool AddState(TKey key, IState state) => _stateMachine.AddState(key, state);

        public bool RemoveState(TKey key) => _stateMachine.RemoveState(key);

        public bool ContainsState(TKey key) => _stateMachine.ContainsState(key);

        public bool TryChangeState(TKey key) => _stateMachine.TryChangeState(key);

        public void ChangeState(TKey key) => _stateMachine.ChangeState(key);

        public bool AddTransition(IStateTransition<TKey> transition)
        {
            if (transition == null)
                throw new ArgumentNullException(nameof(transition));

            if (_transitions.Contains(transition))
                return false;

            _transitions.Add(transition);
            OnTransitionAdded?.Invoke(transition);
            return true;
        }

        public bool AddTransition(TKey from, TKey to, Func<bool> condition)
        {
            if (ContainsTransition(from, to))
                return false;

            var transition = new StateTransition<TKey>(from, to, condition);

            _transitions.Add(transition);
            OnTransitionAdded?.Invoke(transition);
            return true;
        }

        public bool RemoveTransition(IStateTransition<TKey> transition)
        {
            if (!_transitions.Remove(transition))
                return false;

            OnTransitionRemoved?.Invoke(transition);
            return true;
        }

        public bool RemoveTransition(TKey from, TKey to) =>
            FindTransition(from, to, out IStateTransition<TKey> transition) && RemoveTransition(transition);

        public bool ContainsTransition(IStateTransition<TKey> transition) => _transitions.Contains(transition);

        public bool ContainsTransition(TKey from, TKey to) => FindTransition(from, to, out var _);

        private void UpdateTransitions()
        {
            if (_transitions.Count <= 0)
                return;

            var currentState = _stateMachine.CurrentState;

            foreach (var transition in _transitions)
            {
                if (!_comparer.Equals(transition.From, currentState))
                    continue;

                if (!transition.CanPerform())
                    continue;

                _stateMachine.ChangeState(transition.To);
                break;
            }
        }

        private bool FindTransition(TKey from, TKey to, out IStateTransition<TKey> result)
        {
            foreach (var transition in _transitions)
            {
                if (_comparer.Equals(from, transition.From) && _comparer.Equals(to, transition.To))
                {
                    result = transition;
                    return true;
                }
            }

            result = null;
            return false;
        }

        private IEnumerable<(TKey, TKey)> GetTransitions()
        {
            foreach (var transition in _transitions)
                yield return new(transition.From, transition.To);
        }
    }
}