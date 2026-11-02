using System;
using System.Collections.Generic;
using UnityEngine;

namespace WeaponModule
{
    /// <summary>
    /// Objects shown on the weapon view while a modification is installed, and an optional aim pose it requires.
    /// </summary>
    [Serializable]
    public class WeaponModificationVisual
    {
        [SerializeField] private WeaponModificationConfig modification;
        [SerializeField] private GameObject[] visuals;
        [SerializeField] private bool overridesAimPose;
        [SerializeField] private Vector3 aimPosition;
        [SerializeField] private Vector3 aimRotation;

        public WeaponModificationConfig Modification => modification;
        public IReadOnlyList<GameObject> Visuals => visuals ?? Array.Empty<GameObject>();
        public bool OverridesAimPose => overridesAimPose;
        public Vector3 AimPosition => aimPosition;
        public Vector3 AimRotation => aimRotation;
    }
}
