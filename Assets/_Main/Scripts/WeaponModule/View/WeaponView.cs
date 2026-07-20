using SharedData;
using UnityEngine;

namespace WeaponModule
{
    public class WeaponView : MonoBehaviour
    {
        [Header("References")] [SerializeField] private Animator handsAnimator;
        [SerializeField] private Animator gunAnimator;
        [SerializeField] private ParticleSystem muzzleFlash;
        [SerializeField] private Transform firePoint;
        [SerializeField] private Transform aimPivot;

        [Header("Recoil Targets")] [SerializeField] private Transform recoilPivot;
        [SerializeField] private GameObject bulletPrefab;

        private readonly RecoilProcessor _weaponRecoil = new();
        private readonly RecoilProcessor _cameraRecoil = new();
        private readonly SwayProcessor _sway = new();

        private Vector3 _hipPosition;
        private Quaternion _hipRotation;
        private WeaponConfig _config;
        private float _currentAimBlend = 0f;
        private CharacterController _playerCharacter;

        private Transform _cameraTransform;
        private bool _isPaused;
        private bool _wasMuzzleFlashPlaying;

        public void Initialize(WeaponConfig config)
        {
            _config = config;
            _hipPosition = aimPivot.localPosition;
            _hipRotation = aimPivot.localRotation;

            //TODO: Через SerializeField
            _playerCharacter = GetComponentInParent<CharacterController>();
            _cameraTransform = Camera.main!.transform.parent;
        }

        //FIXME: Unused method
        public void SetActive(bool isActive) => gameObject.SetActive(isActive);

        public void SetPaused(bool isPaused)
        {
            _isPaused = isPaused;

            if (handsAnimator != null)
                handsAnimator.speed = isPaused ? 0f : 1f;

            if (gunAnimator != null)
                gunAnimator.speed = isPaused ? 0f : 1f;

            if (muzzleFlash != null)
            {
                if (isPaused)
                {
                    _wasMuzzleFlashPlaying = muzzleFlash.isPlaying;
                    muzzleFlash.Pause(true);
                }
                else if (_wasMuzzleFlashPlaying)
                {
                    muzzleFlash.Play(true);
                }
            }
        }

        public void SetTriggerHold(bool isHeld)
        {
            handsAnimator?.SetBool(WeaponAnimatorHashes.TriggerHold, isHeld);
        }

        public void PlayFireEffects(bool isAiming)
        {
            //FIXME: Magic numbers
            gunAnimator?.Play(WeaponAnimatorHashes.FireState, 1, 0f);

            if (muzzleFlash)
                muzzleFlash.Play();

            if (isAiming)
            {
                _weaponRecoil.Fire(_config.AimRecoil);
                _cameraRecoil.FireCamera(_config.CamAimRecoil);
            }
            else
            {
                _weaponRecoil.Fire(_config.HipRecoil);
                _cameraRecoil.FireCamera(_config.CamHipRecoil);
            }
        }

        public void PlayReload(bool isEmptyReload)
        {
            int trigger = isEmptyReload && _config.UseEmptyReloadAnimation
                ? WeaponAnimatorHashes.EmptyReload
                : WeaponAnimatorHashes.Reload;

            handsAnimator?.SetTrigger(trigger);
            gunAnimator?.SetTrigger(trigger);
        }

        public void SetHolsterState(bool isHolstered)
        {
            var trigger = isHolstered ? WeaponAnimatorHashes.Holster : WeaponAnimatorHashes.Draw;

            handsAnimator?.SetTrigger(trigger);
            gunAnimator?.SetTrigger(trigger);
        }

        public void SetMovementState(bool isMoving, Vector2 inputVector)
        {
            //FIXME: Magic numbers
            if (handsAnimator)
            {
                handsAnimator.SetBool(WeaponAnimatorHashes.IsMoving, isMoving);

                handsAnimator.SetFloat(WeaponAnimatorHashes.InputX, inputVector.x, 0.1f, Time.deltaTime);
                handsAnimator.SetFloat(WeaponAnimatorHashes.InputY, inputVector.y, 0.1f, Time.deltaTime);
            }

            if (!gunAnimator)
                return;

            gunAnimator.SetBool(WeaponAnimatorHashes.IsMoving, isMoving);
            gunAnimator.SetFloat(WeaponAnimatorHashes.InputX, inputVector.x, 0.1f, Time.deltaTime);
            gunAnimator.SetFloat(WeaponAnimatorHashes.InputY, inputVector.y, 0.1f, Time.deltaTime);
        }


        public void UpdateAiming(bool isAiming, float stabilityTarget)
        {
            Vector3 targetPos = isAiming ? _config.AimPosition : _hipPosition;
            Quaternion targetRot = isAiming ? Quaternion.Euler(_config.AimRotation) : _hipRotation;

            float speed = _config.AimSpeed * Time.deltaTime;
            aimPivot.localPosition = Vector3.Lerp(aimPivot.localPosition, targetPos, speed);
            aimPivot.localRotation = Quaternion.Slerp(aimPivot.localRotation, targetRot, speed);
            float targetBlend = isAiming ? stabilityTarget : 0f;

            _currentAimBlend = Mathf.Lerp(_currentAimBlend, targetBlend, Time.deltaTime * 10f);
            if (handsAnimator) handsAnimator.SetFloat(WeaponAnimatorHashes.AimBlend, _currentAimBlend);
            if (gunAnimator) gunAnimator.SetFloat(WeaponAnimatorHashes.AimBlend, _currentAimBlend);
        }

        public void UpdateProcedural(float deltaTime, Vector2 lookInput, bool isAiming)
        {
            if (_isPaused)
                return;

            _weaponRecoil.Update(deltaTime);
            _cameraRecoil.Update(deltaTime);
            _sway.Update(lookInput, _config.Sway, deltaTime, isAiming);
            recoilPivot.localPosition = _weaponRecoil.CurrentPosition + _sway.OutputPosition;
            recoilPivot.localRotation = Quaternion.Euler(_weaponRecoil.CurrentRotation) * _sway.OutputRotation;

            if (_cameraTransform != null)
            {
                _cameraTransform.localRotation = Quaternion.Euler(_cameraRecoil.CurrentRotation);
            }
        }

        public void SpawnBullet(float damage, float speed)
        {
            if (bulletPrefab == null || firePoint == null) return;

            GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            if (bulletObj.TryGetComponent(out Bullet bulletScript))
            {
                Vector3 playerVelocity = _playerCharacter != null ? _playerCharacter.velocity : Vector3.zero;
                bulletScript.Setup(damage, speed, _config.InheritVelocity, playerVelocity);
            }
        }
    }
}
