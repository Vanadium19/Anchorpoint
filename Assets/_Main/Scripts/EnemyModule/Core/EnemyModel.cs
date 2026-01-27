using System;
using UnityEngine;

namespace EnemyModule
{
    public class EnemyModel
    {
        private AIState _currentState;
        private int _currentAmmo;
        private Vector3 _lastKnownPosition;
        private bool _hasTarget;

        public event Action<AIState> StateChanged;

        public AIState CurrentState => _currentState;
        public int CurrentAmmo => _currentAmmo;
        public Vector3 LastKnownPosition => _lastKnownPosition;
        public bool HasTarget => _hasTarget;

        public void Initialize(int maxAmmo, Vector3 startPos)
        {
            _currentAmmo = maxAmmo;
            _lastKnownPosition = startPos;
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

        public bool TryConsumeAmmo()
        {
            if (_currentAmmo <= 0)
                return false;

            _currentAmmo--;
            return true;
        }

        public void Reload(int maxAmmo) => _currentAmmo = maxAmmo;
    }
}