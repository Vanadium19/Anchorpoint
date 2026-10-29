using UnityEngine;

namespace WeaponModule
{
    [CreateAssetMenu(fileName = "WeaponConfig", menuName = "Configs/Weapon")]
    public class WeaponConfig : ScriptableObject
    {
        [Header("Stats")]
        [SerializeField] private float damage = 20f;
        [SerializeField] private float fireRate = 0.1f;
        [SerializeField] private int magazineCapacity = 30;
        [SerializeField] private int initialReserveAmmo = 90;
        [SerializeField] private float bulletSpeed = 200f;

        [Header("Physics")]
        [Range(0f, 1f)]
        [SerializeField] private float inheritVelocity = 0.5f;

        [Header("Timings")]
        [SerializeField] private float reloadTime = 2.0f;
        [SerializeField] private float drawTime = 1.0f;
        [SerializeField] private float emptyReloadDelay = 0.35f;

        [Header("Reload")]
        [SerializeField] private bool useEmptyReloadAnimation = true;

        [Header("Aiming")]
        [SerializeField] private Vector3 aimPosition;
        [SerializeField] private Vector3 aimRotation;
        [SerializeField] private float aimSpeed = 10f;
        [SerializeField] private bool aimIsToggle = false;
        [Range(0f, 1f)]
        [SerializeField] private float aimStability = 0.8f;

        [Header("Ammo")]
        [SerializeField] private AmmoType ammoType = AmmoType.Rifle;

        [Header("Fire Mode")]
        [SerializeField] private bool isAutomatic = true;
        [Min(0f)]
        [SerializeField] private float fireInputBuffer = 0.25f;
        [Min(1)]
        [SerializeField] private int projectilesPerShot = 1;
        [Min(0f)]
        [SerializeField] private float spreadAngle = 0f;

        [Header("Accuracy")]
        [Tooltip("Random deviation of the whole shot in degrees when firing from the hip.")]
        [Min(0f)]
        [SerializeField] private float hipInaccuracy = 0f;
        [Tooltip("Random deviation of the whole shot in degrees when aiming.")]
        [Min(0f)]
        [SerializeField] private float aimInaccuracy = 0f;

        [Header("Reload Mode")]
        [SerializeField] private ReloadMode reloadMode = ReloadMode.Magazine;
        [SerializeField] private float roundReloadTime = 0.5f;

        [Header("--- Recoil & Sway ---")]
        public RecoilSettings HipRecoil;
        public RecoilSettings AimRecoil;
        public CameraRecoilSettings CamHipRecoil;
        public CameraRecoilSettings CamAimRecoil;
        public SwaySettings Sway;

        public float Damage => damage;
        public float FireRate => fireRate;
        public int MagazineCapacity => magazineCapacity;
        public int InitialReserveAmmo => initialReserveAmmo;
        public float BulletSpeed => bulletSpeed;
        public float InheritVelocity => inheritVelocity;
        public float ReloadTime => reloadTime;
        public float DrawTime => drawTime;
        public float EmptyReloadDelay => emptyReloadDelay;
        public bool UseEmptyReloadAnimation => useEmptyReloadAnimation;

        public Vector3 AimPosition => aimPosition;
        public Vector3 AimRotation => aimRotation;
        public float AimSpeed => aimSpeed;
        public bool AimIsToggle => aimIsToggle;
        public float AimStability => aimStability;

        public AmmoType AmmoType => ammoType;
        public bool IsAutomatic => isAutomatic;
        public float FireInputBuffer => fireInputBuffer;
        public int ProjectilesPerShot => projectilesPerShot;
        public float SpreadAngle => spreadAngle;
        public float HipInaccuracy => hipInaccuracy;
        public float AimInaccuracy => aimInaccuracy;
        public ReloadMode ReloadMode => reloadMode;
        public float RoundReloadTime => roundReloadTime;
    }
}