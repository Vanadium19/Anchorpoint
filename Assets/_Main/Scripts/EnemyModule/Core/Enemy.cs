using System;
using UnityEngine;

namespace EnemyModule
{
    public class Enemy
    {
        private AIState _currentState;
        private Vector3 _lastKnownPosition;
        private bool _hasTarget;

        public event Action<AIState> StateChanged;

        public AIState CurrentState => _currentState;
        public Vector3 LastKnownPosition => _lastKnownPosition;
        public bool HasTarget => _hasTarget;

        public void Initialize(Vector3 startPos)
        {
            _lastKnownPosition = startPos;
            _hasTarget = false;
            SetState(AIState.Patrol);
        }

        public void SetState(AIState newState)
        {
            if (_currentState == newState)
                return;

            _currentState = newState;
            StateChanged?.Invoke(newState);
        }

        public void SetTargetPosition(Vector3 pos)
        {
            _lastKnownPosition = pos;
            _hasTarget = true;
        }

        //FIXME: Unused method
        public void ClearTarget() => _hasTarget = false;
    }
}
