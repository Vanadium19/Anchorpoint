using UnityEngine;
using VFXModule;
using Zenject;

namespace WeaponModule
{
    public class WeaponView : MonoBehaviour
    {
        [Header("References")] [SerializeField] private Animator handsAnimator;
        [SerializeField] private Animator gunAnimator;
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

        private IEffectsService _effectsService;
        private DiContainer _container;

        [Inject]
        public void Construct(IEffectsService effectsService, DiContainer container)
        {
            _effectsService = effectsService;
            _container = container;
        }
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

        public void SetTriggerHold(bool isHeld)
        {
            //TODO: Вынести в отдельный класс параметры аниматора и делать через хэш
            handsAnimator?.SetBool("TriggerHold", isHeld);
        }

        public void PlayFireEffects(bool isAiming)
        {
            gunAnimator?.Play("Fire", 1, 0f);
            _effectsService.Fire(EffectId.Shoot, firePoint.position, firePoint.rotation);

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

        public void PlayReload()
        {
            //TODO: Вынести в отдельный класс параметры аниматора и делать через хэш
            handsAnimator?.SetTrigger("Reload");
            gunAnimator?.SetTrigger("Reload");
        }

        public void SetHolsterState(bool isHolstered)
        {
            //TODO: Вынести в отдельный класс параметры аниматора и делать через хэш
            string trigger = isHolstered ? "Holster" : "Draw";

            handsAnimator?.SetTrigger(trigger);
            gunAnimator?.SetTrigger(trigger);
        }

        public void SetMovementState(bool isMoving, Vector2 inputVector)
        {
            //TODO: Вынести в отдельный класс параметры аниматора и делать через хэш
            //FIXME: Magic numbers
            if (handsAnimator)
            {
                handsAnimator.SetBool("IsMoving", isMoving);

                handsAnimator.SetFloat("InputX", inputVector.x, 0.1f, Time.deltaTime);
                handsAnimator.SetFloat("InputY", inputVector.y, 0.1f, Time.deltaTime);
            }

            if (!gunAnimator)
                return;

            gunAnimator.SetBool("IsMoving", isMoving);
            gunAnimator.SetFloat("InputX", inputVector.x, 0.1f, Time.deltaTime);
            gunAnimator.SetFloat("InputY", inputVector.y, 0.1f, Time.deltaTime);
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
            if (handsAnimator) handsAnimator.SetFloat("AimBlend", _currentAimBlend);
            if (gunAnimator) gunAnimator.SetFloat("AimBlend", _currentAimBlend);
        }

        public void UpdateProcedural(float deltaTime, Vector2 lookInput, bool isAiming)
        {
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

            GameObject bulletObj = _container.InstantiatePrefab(
            bulletPrefab,
            firePoint.position,
            firePoint.rotation,
            null
            );

            if (bulletObj.TryGetComponent(out Bullet bulletScript))
            {
                Vector3 playerVelocity = _playerCharacter != null ? _playerCharacter.velocity : Vector3.zero;
                bulletScript.Setup(damage, speed, _config.InheritVelocity, playerVelocity);
            }
        }
    }
}