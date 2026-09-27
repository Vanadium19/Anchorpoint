using System;
using ComponentsModule;
using UnityEngine;

namespace BuildingModule
{
    public class BuildingModel : IDamageable, IBuildingRepairable
    {
        private readonly float _maxHealth;

        private float _currentHealth;
        private float _constructionRemainingTime;
        private BuildingState _state;

        public event Action<float, float> HealthChanged;
        public event Action<BuildingState> StateChanged;

        public BuildingModel(float maxHealth, float constructionTime)
        {
            _maxHealth = Mathf.Max(maxHealth, 1f);
            _currentHealth = _maxHealth;
            _constructionRemainingTime = Mathf.Max(constructionTime, 0f);
            _state = _constructionRemainingTime > 0f
                ? BuildingState.Construction
                : BuildingState.Active;
        }

        public float MaxHealth => _maxHealth;
        public float CurrentHealth => _currentHealth;
        public float ConstructionRemainingTime => _constructionRemainingTime;
        public BuildingState State => _state;
        public bool IsAlive => _state != BuildingState.Broken;
        public bool CanRepair => _state == BuildingState.Broken;

        public void TakeDamage(float amount, Vector3? hitPoint = null, Vector3? force = null)
        {
            if (_state == BuildingState.Broken || amount <= 0f)
                return;

            _currentHealth = Mathf.Max(_currentHealth - amount, 0f);
            HealthChanged?.Invoke(_currentHealth, _maxHealth);

            if (_currentHealth <= 0f)
                SetState(BuildingState.Broken);
        }

        public void Repair()
        {
            if (_state != BuildingState.Broken)
                return;

            _currentHealth = _maxHealth;
            _constructionRemainingTime = 0f;
            HealthChanged?.Invoke(_currentHealth, _maxHealth);
            SetState(BuildingState.Active);
        }

        public void TickConstruction(float deltaTime)
        {
            if (_state != BuildingState.Construction || deltaTime <= 0f)
                return;

            _constructionRemainingTime = Mathf.Max(_constructionRemainingTime - deltaTime, 0f);

            if (_constructionRemainingTime <= 0f)
                SetState(BuildingState.Active);
        }

        public void Restore(BuildingState state, float currentHealth, float constructionRemainingTime)
        {
            _currentHealth = Mathf.Clamp(currentHealth, 0f, _maxHealth);
            _constructionRemainingTime = Mathf.Max(constructionRemainingTime, 0f);

            if (_currentHealth <= 0f || state == BuildingState.Broken)
            {
                _currentHealth = 0f;
                _constructionRemainingTime = 0f;
                SetState(BuildingState.Broken);
                HealthChanged?.Invoke(_currentHealth, _maxHealth);
                return;
            }

            if (state == BuildingState.Construction && _constructionRemainingTime > 0f)
                SetState(BuildingState.Construction);
            else
                SetState(BuildingState.Active);

            HealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        private void SetState(BuildingState state)
        {
            if (_state == state)
                return;

            _state = state;
            StateChanged?.Invoke(_state);
        }
    }
}
