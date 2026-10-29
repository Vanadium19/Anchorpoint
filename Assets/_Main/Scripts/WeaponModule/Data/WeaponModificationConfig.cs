using UnityEngine;

namespace WeaponModule
{
    /// <summary>
    /// A weapon modification and its effect on weapon stats. Which weapons accept it is set
    /// in <see cref="WeaponItemSo"/>; how it looks on a weapon is set in that weapon's prefab.
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponModificationConfig", menuName = "Configs/Weapon Modification")]
    public class WeaponModificationConfig : ScriptableObject
    {
        [Tooltip("Stable save key. Never change it after release.")]
        [SerializeField] private string id;
        [SerializeField] private WeaponModificationSlot slot;

        [Header("Presentation")]
        [SerializeField] private string titleKey;
        [SerializeField] private string descriptionKey;
        [SerializeField] private Sprite icon;

        [Header("Effects")]
        [Min(0f)]
        [SerializeField] private float damageMultiplier = 1f;
        [Min(0)]
        [SerializeField] private int magazineCapacityBonus;
        [Tooltip("Multiplies the shot deviation while aiming. Lower is more accurate.")]
        [Min(0f)]
        [SerializeField] private float aimInaccuracyMultiplier = 1f;

        public string ID => id;
        public WeaponModificationSlot Slot => slot;
        public string TitleKey => titleKey;
        public string DescriptionKey => descriptionKey;
        public Sprite Icon => icon;
        public float DamageMultiplier => damageMultiplier;
        public int MagazineCapacityBonus => magazineCapacityBonus;
        public float AimInaccuracyMultiplier => aimInaccuracyMultiplier;
    }
}
