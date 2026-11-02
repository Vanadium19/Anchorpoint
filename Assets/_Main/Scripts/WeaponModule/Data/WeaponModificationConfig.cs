using System;
using System.Collections.Generic;
using UnityEngine;

namespace WeaponModule
{
    /// <summary>
    /// A permanent weapon modification: the slot it occupies, the stat modifiers it adds and its install cost.
    /// Compatibility is defined by <see cref="WeaponItemSo.AvailableModifications"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponModification", menuName = "Configs/Weapon Modification")]
    public class WeaponModificationConfig : ScriptableObject
    {
        [Tooltip("Stable id stored in saves. Never change it after release.")]
        [SerializeField] private string id;
        [SerializeField] private string nameKey;
        [SerializeField] private string descriptionKey;
        [SerializeField] private Sprite icon;
        [SerializeField] private WeaponModificationSlot slot;
        [SerializeField] private WeaponStatModifier[] modifiers = Array.Empty<WeaponStatModifier>();
        [SerializeField] private WeaponModificationCost[] costs = Array.Empty<WeaponModificationCost>();

        public string Id => id;
        public string NameKey => nameKey;
        public string DescriptionKey => descriptionKey;
        public Sprite Icon => icon;
        public WeaponModificationSlot Slot => slot;
        public IReadOnlyList<WeaponStatModifier> Modifiers => modifiers ?? Array.Empty<WeaponStatModifier>();
        public IReadOnlyList<WeaponModificationCost> Costs => costs ?? Array.Empty<WeaponModificationCost>();
    }
}
