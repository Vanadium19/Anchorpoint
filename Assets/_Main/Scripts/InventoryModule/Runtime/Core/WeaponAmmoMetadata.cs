using System;

namespace InventoryModule
{
    [Serializable]
    public class WeaponAmmoMetadata : InventoryMetadata
    {
        public int CurrentMagazineAmmo { get; set; }
        public int ReserveAmmo { get; set; }

        public void SetInitialValues(int magazineAmmo, int reserveAmmo)
        {
            CurrentMagazineAmmo = magazineAmmo;
            ReserveAmmo = reserveAmmo;
        }
    }
}
