using UnityEngine;
using WeaponModule.Configs;
using WeaponModule.Content;
using WeaponModule.View.Procedural;

namespace WeaponModule.View
{
    public class WeaponView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator handsAnimator;
        [SerializeField] private Animator gunAnimator;
        [SerializeField] private ParticleSystem muzzleFlash;
        [SerializeField] private Transform firePoint;
        [SerializeField] private Transform aimPivot;

        [Header("Recoil Targets")]
        [SerializeField] private Transform recoilPivot;

        // Ссылка на префаб пули (лучше через пул, но пока так)
        [SerializeField] private GameObject bulletPrefab;

        private Vector3 _hipPosition;
        private Quaternion _hipRotation;
        private WeaponConfig _config;
        private float _currentAimBlend = 0f;
        private CharacterController _playerCharacter;

        private Transform _cameraTransform;
        private RecoilProcessor _weaponRecoil = new();
        private RecoilProcessor _cameraRecoil = new();
        private SwayProcessor _sway = new();


        public void Initialize(WeaponConfig config)
        {
            _config = config;
            _hipPosition = aimPivot.localPosition;
            _hipRotation = aimPivot.localRotation;
            _playerCharacter = GetComponentInParent<CharacterController>();
            if (Camera.main != null)
                _cameraTransform = Camera.main.transform.parent;
        }

        // --- Действия ---
        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        public void SetTriggerHold(bool isHeld)
        {
            if (handsAnimator) handsAnimator.SetBool("TriggerHold", isHeld);
        }
        public void PlayFireEffects(bool isAiming)
        {
            if (gunAnimator) gunAnimator.Play("Fire", 1, 0f);
            if (muzzleFlash) muzzleFlash.Play();
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
            if (handsAnimator) handsAnimator.SetTrigger("Reload");
            if (gunAnimator) gunAnimator.SetTrigger("Reload");
        }

        public void SetHolsterState(bool isHolstered)
        {
            string trigger = isHolstered ? "Holster" : "Draw";
            if (handsAnimator) handsAnimator.SetTrigger(trigger);
            if (gunAnimator) gunAnimator.SetTrigger(trigger);
        }

        public void SetMovementState(bool isMoving, Vector2 inputVector)
        {
            if (handsAnimator)
            {
                // 1. Основное состояние (бежим или нет)
                handsAnimator.SetBool("IsMoving", isMoving);

                // 2. Для Blend Tree (качание оружия)
                // 0.1f - это время сглаживания (dampTime), чтобы анимация была плавной
                handsAnimator.SetFloat("InputX", inputVector.x, 0.1f, Time.deltaTime);
                handsAnimator.SetFloat("InputY", inputVector.y, 0.1f, Time.deltaTime);
            }

            // Если у пушки есть свой аниматор, передаем и туда
            if (gunAnimator)
            {
                gunAnimator.SetBool("IsMoving", isMoving);
                gunAnimator.SetFloat("InputX", inputVector.x, 0.1f, Time.deltaTime);
                gunAnimator.SetFloat("InputY", inputVector.y, 0.1f, Time.deltaTime);
            }
        }

        // --- Прицеливание (Update Logic) ---
        // View сама интерполирует позицию, Контроллер только говорит "Прицелься"

        public void UpdateAiming(bool isAiming, float stabilityTarget)
        {
            // 1. Двигаем сам объект (как и раньше)
            Vector3 targetPos = isAiming ? _config.AimPosition : _hipPosition;
            Quaternion targetRot = isAiming ? Quaternion.Euler(_config.AimRotation) : _hipRotation;

            float speed = _config.AimSpeed * Time.deltaTime;
            aimPivot.localPosition = Vector3.Lerp(aimPivot.localPosition, targetPos, speed);
            aimPivot.localRotation = Quaternion.Slerp(aimPivot.localRotation, targetRot, speed);

            // 2. Считаем Blend для аниматора (Стабильность)
            // Если целимся -> идем к aimStability (0.8), иначе -> к 0.
            float targetBlend = isAiming ? stabilityTarget : 0f;

            // Плавно меняем значение
            _currentAimBlend = Mathf.Lerp(_currentAimBlend, targetBlend, Time.deltaTime * 10f);

            // Передаем в аниматоры
            if (handsAnimator) handsAnimator.SetFloat("AimBlend", _currentAimBlend);
            if (gunAnimator) gunAnimator.SetFloat("AimBlend", _currentAimBlend);
        }
        public void UpdateProcedural(float deltaTime, Vector2 lookInput, bool isAiming)
        {
            // 1. Считаем математику
            _weaponRecoil.Update(deltaTime);
            _cameraRecoil.Update(deltaTime);
            _sway.Update(lookInput, _config.Sway, deltaTime, isAiming);

            // 2. Применяем к ОРУЖИЮ (RecoilPivot)
            // Складываем позицию отдачи и позицию sway
            recoilPivot.localPosition = _weaponRecoil.CurrentPosition + _sway.OutputPosition;
            recoilPivot.localRotation = Quaternion.Euler(_weaponRecoil.CurrentRotation) * _sway.OutputRotation;

            // 3. Применяем к КАМЕРЕ (Если нашли её)
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
                // Получаем скорость игрока (если контроллер найден)
                Vector3 playerVelocity = _playerCharacter != null ? _playerCharacter.velocity : Vector3.zero;

                // Передаем всё в пулю
                bulletScript.Setup(damage, speed, _config.InheritVelocity, playerVelocity);
            }
        }
    }
}