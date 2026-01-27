using UnityEngine;

namespace WeaponModule
{
    [System.Serializable]
    public class RecoilSettings
    {
        public Vector3 RecoilRotation = new Vector3(-2f, 2f, 2f);
        public float KickBackZ = 0.2f;
        public float Snappiness = 6f;
        public float ReturnSpeed = 2f;
    }
}