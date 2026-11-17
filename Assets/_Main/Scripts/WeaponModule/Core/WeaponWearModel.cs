using System;
using UnityEngine;

namespace WeaponModule
{
    public class WeaponWearModel
    {
        public const float MaxWear = 100f;

        private const float PercentScale = 100f;

        private readonly WeaponWearSettings _settings;

        private float _wear;
        private float _multiplier;
        private WeaponMalfunction _malfunction;

        public event Action<float> WearChanged;
        public event Action<WeaponMalfunction> MalfunctionChanged;

        public WeaponWearModel(WeaponWearSettings settings)
        {
            _settings = settings;
            _multiplier = settings.WearMultiplier;
        }

        public float Wear => _wear;
        public float Multiplier => _multiplier;
        public WeaponMalfunction Malfunction => _malfunction;
        public bool HasMalfunction => _malfunction != WeaponMalfunction.None;

        public void SetMultiplier(float multiplier) => _multiplier = Mathf.Max(0f, multiplier);

        public void RegisterShot()
        {
            var newWear = Mathf.Min(MaxWear, _wear + _settings.WearPerShot * _multiplier);

            if (Mathf.Approximately(newWear, _wear))
                return;

            _wear = newWear;
            WearChanged?.Invoke(_wear);
        }

        /// <summary>
        /// Returns the malfunction chance in percent for the current wear.
        /// </summary>
        public float GetMalfunctionChance()
        {
            var threshold = _settings.MalfunctionThreshold;

            if (_wear <= threshold)
                return 0f;

            var wearRange = MaxWear - threshold;

            if (wearRange <= 0f)
                return _settings.MaxMalfunctionChance;

            return _settings.MaxMalfunctionChance * (_wear - threshold) / wearRange;
        }

        /// <summary>
        /// Rolls a malfunction for one trigger pull. Both rolls are in the range 0-1.
        /// </summary>
        public bool TryRollMalfunction(float chanceRoll, float kindRoll)
        {
            if (HasMalfunction)
                return false;

            if (chanceRoll * PercentScale >= GetMalfunctionChance())
                return false;

            SetMalfunction(PickMalfunction(kindRoll));
            return true;
        }

        public void ClearMalfunction() => SetMalfunction(WeaponMalfunction.None);

        private WeaponMalfunction PickMalfunction(float kindRoll)
        {
            if (_settings.JamWeight <= 0f)
                return WeaponMalfunction.Misfire;

            if (_settings.MisfireWeight <= 0f)
                return WeaponMalfunction.Jam;

            var totalWeight = _settings.MisfireWeight + _settings.JamWeight;

            return kindRoll * totalWeight < _settings.MisfireWeight
                ? WeaponMalfunction.Misfire
                : WeaponMalfunction.Jam;
        }

        private void SetMalfunction(WeaponMalfunction malfunction)
        {
            if (_malfunction == malfunction)
                return;

            _malfunction = malfunction;
            MalfunctionChanged?.Invoke(_malfunction);
        }
    }
}
