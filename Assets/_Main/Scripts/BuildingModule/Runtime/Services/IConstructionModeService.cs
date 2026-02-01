using System;

namespace BuildingModule
{
    public interface IConstructionModeService
    {
        event Action<bool> ActiveChanged;
        bool IsActive { get; }

        void Toggle();

        void SetActive(bool value);
    }
}