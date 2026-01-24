using System;

namespace BuildingModule
{
    public interface IConstructionModeService
    {
        event Action<bool> ActiveChanged;

        void Toggle();

        void SetActive(bool value);
    }
}