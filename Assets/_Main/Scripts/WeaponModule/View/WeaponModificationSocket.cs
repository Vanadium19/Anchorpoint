using System;
using System.Collections.Generic;
using UnityEngine;

namespace WeaponModule
{
    /// <summary>
    /// A place on a weapon view that takes the modification of one socket type. An attachment of the modification
    /// is created under the mount and hides the stock parts, a part operation reshapes the stock parts in place.
    /// </summary>
    [Serializable]
    public class WeaponModificationSocket
    {
        [Tooltip("Interface of this socket. A modification of the same type fits here.")]
        [SerializeField] private WeaponSocketType type;
        [Tooltip("Parent of the attachment and origin of part operations. +Z along the bore, +Y up.")]
        [SerializeField] private Transform mount;
        [Tooltip("Objects of the stock weapon that the attachment replaces or the part operation reshapes.")]
        [SerializeField] private GameObject[] stockParts;
        [Tooltip("Hardware of the weapon itself that joins an attachment to it. Created under the mount while an attachment is shown. Optional.")]
        [SerializeField] private GameObject adapterPrefab;
        [Tooltip("Distance in metres from the camera to the aim point while aiming. Used by sights on this socket.")]
        [Min(0.01f)]
        [SerializeField] private float eyeRelief = 0.3f;
        [Tooltip("Free room in metres above the mount axis for part operations. 0 means unlimited.")]
        [Min(0f)]
        [SerializeField] private float clearanceAboveAxis;
        [Tooltip("Free room in metres below the mount axis for part operations. 0 means unlimited.")]
        [Min(0f)]
        [SerializeField] private float clearanceBelowAxis;

        public WeaponSocketType Type => type;
        public Transform Mount => mount;
        public IReadOnlyList<GameObject> StockParts => stockParts ?? Array.Empty<GameObject>();
        public GameObject AdapterPrefab => adapterPrefab;
        public float EyeRelief => eyeRelief;
        public float ClearanceAboveAxis => clearanceAboveAxis;
        public float ClearanceBelowAxis => clearanceBelowAxis;
    }
}
