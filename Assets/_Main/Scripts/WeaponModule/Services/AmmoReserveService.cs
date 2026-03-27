using System.Collections.Generic;
using UnityEngine;

namespace WeaponModule
{
    public class AmmoReserveService
    {
        private class AmmoState
        {
            public int CurrentMagazineAmmo;
            public int ReserveAmmo;
        }

        private static readonly Dictionary<string, AmmoState> AmmoByWeaponId = new();

        public void GetAmmoState(string weaponId, int defaultMagazineAmmo, int defaultReserveAmmo,
            out int currentMagazineAmmo, out int reserveAmmo)
        {
            if (AmmoByWeaponId.TryGetValue(weaponId, out AmmoState ammoState))
            {
                currentMagazineAmmo = ammoState.CurrentMagazineAmmo;
                reserveAmmo = ammoState.ReserveAmmo;
                return;
            }

            currentMagazineAmmo = Mathf.Max(0, defaultMagazineAmmo);
            reserveAmmo = Mathf.Max(0, defaultReserveAmmo);

            AmmoByWeaponId[weaponId] = new AmmoState
            {
                CurrentMagazineAmmo = currentMagazineAmmo,
                ReserveAmmo = reserveAmmo
            };
        }

        public void SetAmmoState(string weaponId, int currentMagazineAmmo, int reserveAmmo)
        {
            AmmoByWeaponId[weaponId] = new AmmoState
            {
                CurrentMagazineAmmo = Mathf.Max(0, currentMagazineAmmo),
                ReserveAmmo = Mathf.Max(0, reserveAmmo)
            };
        }
    }
}