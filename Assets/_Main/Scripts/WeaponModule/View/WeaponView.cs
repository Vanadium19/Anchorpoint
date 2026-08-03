using UnityEngine;
using Zenject;

namespace WeaponModule
{
    public class WeaponView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator handsAnimator;
        [SerializeField] private Animator gunAnimator;
        [SerializeField] private ParticleSystem muzzleFlash;
        [SerializeField] private Transform firePoint;
        [SerializeField] private Transform aimPivot;

        [Header("Animation")]
        [SerializeField] private int fireAnimationLayer = 1;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip fireSound;
        [SerializeField] private AudioClip reloadSound;
        [SerializeField] private AudioClip emptyReloadSound;

        [Header("Recoil Targets")]
        [SerializeField] private Transform recoilPivot;
        [SerializeField] private GameObject bulletPrefab;

        private readonly RecoilProcessor _weaponRecoil = new();
        private readonly SwayProcessor _sway = new();

        private ICameraRecoilService _cameraRecoil;
        private Vector3 _hipPosition;
        private Quaternion _hipRotation;
        private WeaponConfig _config;
        private float _currentAimBlend;
        private Transform _cameraTransform;

        public GameObject BulletPrefab => bulletPrefab;
        public Transform FirePoint => firePoint;

        [Inject]
        private void Construct(ICameraRecoilService cameraRecoil)
        {
            _cameraRecoil = cameraRecoil;
        }

        public void Initialize(WeaponConfig config)
        {
            _config = config;

            if (aimPivot != null)
            {
                _hipPosition = aimPivot.localPosition;
                _hipRotation = aimPivot.localRotation;
            }

            var mainCamera = Camera.main;
            _cameraTransform = mainCamera != null ? mainCamera.transform.parent : null;
        }

        public void SetTriggerHold(bool isHeld)
        {
            handsAnimator?.SetBool(WeaponAnimatorHashes.TriggerHold, isHeld);
        }

        public void PlayFireEffects(bool isAiming)
        {
            PlayFireAnimation();

            if (muzzleFlash != null)
                muzzleFlash.Play(true);

            PlaySound(fireSound);

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
            var trigger = isEmptyReload && _config.UseEmptyReloadAnimation
                ? WeaponAnimatorHashes.EmptyReload
                : WeaponAnimatorHashes.Reload;

            handsAnimator?.SetTrigger(trigger);
            gunAnimator?.SetTrigger(trigger);

            var sound = isEmptyReload && emptyReloadSound != null
                ? emptyReloadSound
                : reloadSound;

            PlaySound(sound);
        }

        public void SetHolsterState(bool isHolstered)
        {
            var trigger = isHolstered ? WeaponAnimatorHashes.Holster : WeaponAnimatorHashes.Draw;

            handsAnimator?.SetTrigger(trigger);
            gunAnimator?.SetTrigger(trigger);
        }

        public void SetMovementState(bool isMoving, Vector2 inputVector)
        {
            if (handsAnimator != null)
            {
                handsAnimator.SetBool(WeaponAnimatorHashes.IsMoving, isMoving);
                handsAnimator.SetFloat(WeaponAnimatorHashes.InputX, inputVector.x, 0.1f, Time.deltaTime);
                handsAnimator.SetFloat(WeaponAnimatorHashes.InputY, inputVector.y, 0.1f, Time.deltaTime);
            }

            if (gunAnimator == null)
                return;

            gunAnimator.SetBool(WeaponAnimatorHashes.IsMoving, isMoving);
            gunAnimator.SetFloat(WeaponAnimatorHashes.InputX, inputVector.x, 0.1f, Time.deltaTime);
            gunAnimator.SetFloat(WeaponAnimatorHashes.InputY, inputVector.y, 0.1f, Time.deltaTime);
        }

        public void UpdateAiming(bool isAiming, float stabilityTarget)
        {
            if (aimPivot == null)
                return;

            var targetPosition = isAiming ? _config.AimPosition : _hipPosition;
            var targetRotation = isAiming ? Quaternion.Euler(_config.AimRotation) : _hipRotation;
            var speed = _config.AimSpeed * Time.deltaTime;

            aimPivot.localPosition = Vector3.Lerp(aimPivot.localPosition, targetPosition, speed);
            aimPivot.localRotation = Quaternion.Slerp(aimPivot.localRotation, targetRotation, speed);

            var targetBlend = isAiming ? stabilityTarget : 0f;
            _currentAimBlend = Mathf.Lerp(_currentAimBlend, targetBlend, Time.deltaTime * 10f);

            if (handsAnimator != null)
                handsAnimator.SetFloat(WeaponAnimatorHashes.AimBlend, _currentAimBlend);

            if (gunAnimator != null)
                gunAnimator.SetFloat(WeaponAnimatorHashes.AimBlend, _currentAimBlend);
        }

        public void UpdateProcedural(float deltaTime, Vector2 lookInput, bool isAiming)
        {
            _weaponRecoil.Update(deltaTime);
            _cameraRecoil.Update(deltaTime);
            _sway.Update(lookInput, _config.Sway, deltaTime, isAiming);

            if (recoilPivot != null)
            {
                recoilPivot.localPosition = _weaponRecoil.CurrentPosition + _sway.OutputPosition;
                recoilPivot.localRotation = Quaternion.Euler(_weaponRecoil.CurrentRotation) * _sway.OutputRotation;
            }

            if (_cameraTransform != null)
                _cameraTransform.localRotation = Quaternion.Euler(_cameraRecoil.CurrentRotation);
        }

        public void ResetVisuals()
        {
            _weaponRecoil.Reset();
            _sway.Reset();
            _currentAimBlend = 0f;

            if (aimPivot != null)
            {
                aimPivot.localPosition = _hipPosition;
                aimPivot.localRotation = _hipRotation;
            }

            if (audioSource != null)
                audioSource.Stop();

            if (handsAnimator != null)
                handsAnimator.Rebind();

            if (gunAnimator != null)
                gunAnimator.Rebind();
        }

        private void PlayFireAnimation()
        {
            if (gunAnimator == null || fireAnimationLayer < 0 || fireAnimationLayer >= gunAnimator.layerCount)
                return;

            gunAnimator.Play(WeaponAnimatorHashes.FireState, fireAnimationLayer, 0f);
        }

        private void PlaySound(AudioClip clip)
        {
            if (audioSource == null || clip == null)
                return;

            audioSource.PlayOneShot(clip);
        }
    }
}
