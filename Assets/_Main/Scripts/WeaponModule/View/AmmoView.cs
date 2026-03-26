using TMPro;
using UnityEngine;

namespace WeaponModule
{
    public class AmmoView : MonoBehaviour
    {
        [SerializeField] private TMP_Text ammoText;

        public void SetAmmo(int currentMagazineAmmo, int reserveAmmo)
        {
            if (ammoText == null)
                return;

            ammoText.text = $"{currentMagazineAmmo} / {reserveAmmo}";
        }

        public void Clear()
        {
            if (ammoText == null)
                return;

            ammoText.text = "- / -";
        }
    }
}