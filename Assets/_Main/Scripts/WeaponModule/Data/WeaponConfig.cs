using UnityEngine;

namespace WeaponModule
{
    [CreateAssetMenu(fileName = "WeaponConfig", menuName = "Configs/Weapon")]
    public class WeaponConfig : ScriptableObject
    {
        [Header("Stats")]
        [SerializeField] private float damage = 20f;
        [SerializeField] private float fireRate = 0.1f;
        [SerializeField] private int maxAmmo = 30;
        [SerializeField] private float bulletSpeed = 200f;

        [Header("Physics")] [Range(0f, 1f)]
        [SerializeField] private float inheritVelocity = 0.5f;

        [Header("Timings")]
        [SerializeField] private float reloadTime = 2.0f;
        [SerializeField] private float drawTime = 1.0f;

        [Header("Aiming")]
        [SerializeField]private Vector3 aimPosition;
        [SerializeField] private Vector3 aimRotation;
        [SerializeField] private float aimSpeed = 10f;
        [SerializeField] private bool aimIsToggle = false;
        [Range(0f, 1f)] [SerializeField] private float aimStability = 0.8f;

        [Header("--- Recoil & Sway ---")]
        public RecoilSettings HipRecoil;
        public RecoilSettings AimRecoil;
        public CameraRecoilSettings CamHipRecoil;
        public CameraRecoilSettings CamAimRecoil;
        public SwaySettings Sway;

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
    }
}