using System;

namespace BuildingModule
{
    public class ConstructionModeService : IConstructionModeService
    {
        private bool _isActive;
        public bool IsActive => _isActive;

        public event Action<bool> ActiveChanged;

        public void Toggle() => SetActive(!_isActive);

        public void SetActive(bool value)
        {
            if (_isActive == value) return;
            _isActive = value;
            ActiveChanged?.Invoke(value);
        }
    }
}