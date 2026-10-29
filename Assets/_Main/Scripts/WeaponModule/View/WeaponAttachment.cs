using System;
using UnityEngine;

namespace WeaponModule
{
    /// <summary>
    /// How one modification looks on a weapon prefab.
    /// </summary>
    [Serializable]
    public class WeaponAttachment
    {
        [SerializeField] private WeaponModificationConfig modification;
        [SerializeField] private GameObject[] shownObjects = Array.Empty<GameObject>();
        [Tooltip("Stock parts replaced by the modification, e.g. the default magazine.")]
        [SerializeField] private GameObject[] hiddenObjects = Array.Empty<GameObject>();
        [Tooltip("Added to the aim position so the camera looks through the installed sight.")]
        [SerializeField] private Vector3 aimPositionOffset;

        public WeaponModificationConfig Modification => modification;
        public GameObject[] ShownObjects => shownObjects;
        public GameObject[] HiddenObjects => hiddenObjects;
        public Vector3 AimPositionOffset => aimPositionOffset;
    }
}
