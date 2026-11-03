using System;
using System.Collections.Generic;
using UnityEngine;

namespace WeaponModule
{
    /// <summary>
    /// A permanent weapon modification: the socket type it needs, the stat modifiers it adds, its install cost and
    /// how it looks. A weapon takes it when the view of the weapon has a socket of the same type. The look is an
    /// attachment prefab, a part operation on the stock parts of the socket, both or neither (stat-only).
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponModification", menuName = "Configs/Weapon Modification")]
    public class WeaponModificationConfig : ScriptableObject
    {
        [Tooltip("Stable id stored in saves. Never change it after release.")]
        [SerializeField] private string id;
        [SerializeField] private string nameKey;
        [SerializeField] private string descriptionKey;
        [SerializeField] private Sprite icon;
        [SerializeField] private WeaponSocketType socketType;
        [Tooltip("Object that replaces the stock parts of the socket. Optional.")]
        [SerializeField] private WeaponAttachment attachmentPrefab;
        [Tooltip("Reshapes the stock parts of the socket instead of replacing them. Optional.")]
        [SerializeField] private WeaponPartOperation partOperation;
        [SerializeField] private WeaponStatModifier[] modifiers = Array.Empty<WeaponStatModifier>();
        [SerializeField] private WeaponModificationCost[] costs = Array.Empty<WeaponModificationCost>();

        public string Id => id;
        public string NameKey => nameKey;
        public string DescriptionKey => descriptionKey;
        public Sprite Icon => icon;
        public WeaponSocketType SocketType => socketType;
        public WeaponAttachment AttachmentPrefab => attachmentPrefab;
        public WeaponPartOperation PartOperation => partOperation;
        public IReadOnlyList<WeaponStatModifier> Modifiers => modifiers ?? Array.Empty<WeaponStatModifier>();
        public IReadOnlyList<WeaponModificationCost> Costs => costs ?? Array.Empty<WeaponModificationCost>();
    }
}
