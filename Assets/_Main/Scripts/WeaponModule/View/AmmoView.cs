using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilsModule;

namespace WeaponModule
{
    public class AmmoView : MonoBehaviour
    {
        [SerializeField] private TMP_Text ammoText;

        [Header("Wear")]
        [SerializeField] private Image wearFill;
        [Tooltip("Wear percent from which the indicator turns yellow.")]
        [Range(0f, 100f)]
        [SerializeField] private float mediumWearThreshold = 30f;
        [Tooltip("Wear percent from which the indicator turns red.")]
        [Range(0f, 100f)]
        [SerializeField] private float highWearThreshold = 70f;
        [SerializeField] private Color lowWearColor = new(0.30f, 0.80f, 0.30f);
        [SerializeField] private Color mediumWearColor = new(0.95f, 0.80f, 0.20f);
        [SerializeField] private Color highWearColor = new(0.90f, 0.20f, 0.20f);

        [Header("Malfunction")]
        [SerializeField] private TMP_Text malfunctionText;
        [SerializeField] private string misfireHintKey = "weapon_misfire_hint";
        [SerializeField] private string jamHintKey = "weapon_jam_hint";

        public void SetAmmo(int currentMagazineAmmo, int reserveAmmo)
        {
            if (ammoText == null)
                return;

            ammoText.text = $"{currentMagazineAmmo} / {reserveAmmo}";
        }

        public void SetWear(float wear)
        {
            if (wearFill == null)
                return;

            wearFill.fillAmount = Mathf.Clamp01(wear / WeaponWearModel.MaxWear);
            wearFill.color = GetWearColor(wear);
        }

        public void SetMalfunction(WeaponMalfunction malfunction)
        {
            if (malfunctionText == null)
                return;

            var hintKey = GetHintKey(malfunction);

            malfunctionText.gameObject.SetActive(!string.IsNullOrEmpty(hintKey));
            malfunctionText.text = string.IsNullOrEmpty(hintKey) ? string.Empty : LocalizedText.Get(hintKey);
        }

        public void Clear()
        {
            if (ammoText != null)
                ammoText.text = "- / -";

            SetWear(0f);
            SetMalfunction(WeaponMalfunction.None);
        }

        private Color GetWearColor(float wear)
        {
            if (wear >= highWearThreshold)
                return highWearColor;

            return wear >= mediumWearThreshold ? mediumWearColor : lowWearColor;
        }

        private string GetHintKey(WeaponMalfunction malfunction)
        {
            switch (malfunction)
            {
                case WeaponMalfunction.Misfire:
                    return misfireHintKey;
                case WeaponMalfunction.Jam:
                    return jamHintKey;
                default:
                    return null;
            }
        }
    }
}
