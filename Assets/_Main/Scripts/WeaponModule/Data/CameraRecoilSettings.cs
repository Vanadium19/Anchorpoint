using UnityEngine;

namespace WeaponModule
{
    [System.Serializable]
    public class CameraRecoilSettings
    {
        public Vector3 RecoilAmount = new Vector3(-2f, 2f, 0.5f);
        public float Snappiness = 6f;
        public float ReturnSpeed = 2f;
    }
}