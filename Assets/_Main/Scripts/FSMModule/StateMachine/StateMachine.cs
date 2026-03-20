using System;
using System.Collections.Generic;
using System.Linq;

namespace FSMModule
{
    public class StateMachine<TKey> : IStateMachine<TKey>
    {
        private static readonly EqualityComparer<TKey> _comparer = EqualityComparer<TKey>.Default;

        private readonly Dictionary<TKey, IState> _states;

        private TKey _currentKey;
        private IState _currentState;

        public event Action<TKey> OnStateAdded;
        public event Action<TKey> OnStateChanged;
        public event Action<TKey> OnStateRemoved;

        public StateMachine()
        {
            _states = new();
            _currentKey = default;
            _currentState = null;
        }

        public StateMachine(TKey initialState, params (TKey, IState)[] states)
        {
            if (states == null)
                throw new ArgumentNullException(nameof(states));

            _states = states.ToDictionary(it => it.Item1, it => it.Item2);
            _currentKey = initialState;
            _currentState = _states[_currentKey];
        }

        public StateMachine(TKey initialState, IEnumerable<(TKey, IState)> states)
        {
            if (states == null)
                throw new ArgumentNullException(nameof(states));

            _states = states.ToDictionary(it => it.Item1, it => it.Item2);
            _currentKey = initialState;
            _currentState = _states[_currentKey];
        }

        public StateMachine(TKey initialState, params KeyValuePair<TKey, IState>[] states)
        {
            if (states == null)
                throw new ArgumentNullException(nameof(states));

            _states = new(states);
            _currentKey = initialState;
            _currentState = _states[_currentKey];
        }

        public StateMachine(TKey initialState, IEnumerable<KeyValuePair<TKey, IState>> states)
        {
            if (states == null)
                throw new ArgumentNullException(nameof(states));

            _states = new(states);
            _currentKey = initialState;
            _currentState = _states[_currentKey];
        }

        public int StateCount => _states.Count;
        public TKey CurrentState => _currentKey;
        public IReadOnlyCollection<TKey> States => _states.Keys;

        public void OnEnter() => _currentState?.OnEnter();

        public void OnUpdate(float deltaTime) => _currentState?.OnUpdate(deltaTime);

        public void OnExit() => _currentState?.OnExit();

        public bool AddState(TKey key, IState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            if (!_states.TryAdd(key, state))
                return false;

            OnStateAdded?.Invoke(key);
            return true;
        }

        public bool RemoveState(TKey key)
        {
            if (!_states.Remove(key))
                return false;

            OnStateRemoved?.Invoke(key);
            return true;
        }

        public bool ContainsState(TKey key) => _states.ContainsKey(key);

        public bool TryChangeState(TKey key)
        {
            if (_comparer.Equals(_currentKey, key))
                return false;

            ChangeState(key);
            return true;
        }

        public void ChangeState(TKey key)
        {
            _currentState?.OnExit();

            _currentKey = key;
            _states.TryGetValue(key, out _currentState);
            _currentState?.OnEnter();

            OnStateChanged?.Invoke(key);
        }
    }
}