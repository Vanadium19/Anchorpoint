using UnityEngine;

namespace WeaponModule.Configs
{
    [CreateAssetMenu(fileName = "WeaponConfig", menuName = "Configs/Weapon")]
    public class WeaponConfig : ScriptableObject
    {
        [Header("Stats")]
        [SerializeField] private float damage = 20f;
        [SerializeField] private float fireRate = 0.1f;
        [SerializeField] private int maxAmmo = 30;
        [SerializeField] private float bulletSpeed = 200f;

        [Header("Physics")]
        [Range(0f, 1f)]
        [SerializeField] private float inheritVelocity = 0.5f;

        [Header("Timings")]
        [SerializeField] private float reloadTime = 2.0f;
        [SerializeField] private float drawTime = 1.0f;

        [Header("Aiming")]
        [SerializeField] private Vector3 aimPosition;
        [SerializeField] private Vector3 aimRotation;
        [SerializeField] private float aimSpeed = 10f;
        [SerializeField] private bool aimIsToggle = false;
        [Range(0f, 1f)]
        [SerializeField] private float aimStability = 0.8f;

        // --- НОВЫЕ ПОЛЯ (Recoil & Sway) ---
        // Важно: они должны быть public, чтобы View мог их читать
        [Header("--- Recoil & Sway ---")]
        public RecoilSettings HipRecoil;
        public RecoilSettings AimRecoil;
        public CameraRecoilSettings CamHipRecoil;
        public CameraRecoilSettings CamAimRecoil;
        public SwaySettings Sway;

        // --- Геттеры для приватных полей ---
        public float Damage => damage;
        public float FireRate => fireRate;
        public int MaxAmmo => maxAmmo;
        public float BulletSpeed => bulletSpeed;
        public float InheritVelocity => inheritVelocity;
        public float ReloadTime => reloadTime;
        public float DrawTime => drawTime;

        public Vector3 AimPosition => aimPosition;
        public Vector3 AimRotation => aimRotation;
        public float AimSpeed => aimSpeed;
        public bool AimIsToggle => aimIsToggle;
        public float AimStability => aimStability;

        // --- Вспомогательные классы настроек ---
        // Они должны быть [System.Serializable], чтобы отображаться в Инспекторе

        [System.Serializable]
        public class RecoilSettings
        {
            public Vector3 RecoilRotation = new Vector3(-10f, 2f, 2f); // X, Y, Z
            public float KickBackZ = 0.2f;
            public float Snappiness = 6f;
            public float ReturnSpeed = 2f;
        }

        [System.Serializable]
        public class CameraRecoilSettings
        {
            public Vector3 RecoilAmount = new Vector3(-2f, 2f, 0.5f); // X, Y, Z
            public float Snappiness = 6f;
            public float ReturnSpeed = 2f;
        }

        [System.Serializable]
        public class SwaySettings
        {
            [Header("Position")]
            public float Step = 0.02f;
            public float MaxStep = 0.06f;
            public float Smooth = 8f;

            [Header("Rotation")]
            public float RotationStep = 4f;
            public float MaxRotation = 10f;
            public float SmoothRot = 10f;

            [Header("Tilt (Z)")]
            public float Tilt = 2f;
            public float MaxTilt = 5f;
        }
    }
}