using System.Collections.Generic;
using UnityEngine;

namespace WeaponModule
{
    public class AmmoReserveService
    {
        private readonly Dictionary<string, AmmoState> _ammoByWeaponId = new();

        public void GetAmmoState(
            string weaponId,
            int defaultMagazineAmmo,
            int defaultReserveAmmo,
            out int currentMagazineAmmo,
            out int reserveAmmo)
        {
            if (_ammoByWeaponId.TryGetValue(weaponId, out var ammoState))
            {
                currentMagazineAmmo = ammoState.CurrentMagazineAmmo;
                reserveAmmo = ammoState.ReserveAmmo;
                return;
            }

            currentMagazineAmmo = Mathf.Max(0, defaultMagazineAmmo);
            reserveAmmo = Mathf.Max(0, defaultReserveAmmo);

            _ammoByWeaponId[weaponId] = new AmmoState
            {
                CurrentMagazineAmmo = currentMagazineAmmo,
                ReserveAmmo = reserveAmmo
            };
        }

        public void SetAmmoState(string weaponId, int currentMagazineAmmo, int reserveAmmo)
        {
            _ammoByWeaponId[weaponId] = new AmmoState
            {
                CurrentMagazineAmmo = Mathf.Max(0, currentMagazineAmmo),
                ReserveAmmo = Mathf.Max(0, reserveAmmo)
            };
        }
    }
}