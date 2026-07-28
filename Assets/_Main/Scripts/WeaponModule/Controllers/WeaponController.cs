using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using InventoryModule;
using UnityEngine;
using Zenject;

namespace WeaponModule
{
    public class WeaponController : IWeapon, IInitializable, ITickable, ILateTickable, IDisposable
    {
        private readonly WeaponConfig _config;
        private readonly WeaponModel _model;
        private readonly WeaponView _view;
        private readonly IWeaponStatsProvider _statsProvider;
        private readonly IWeaponViewFactory _viewFactory;
        private readonly IBulletFactory _bulletFactory;
        private readonly Transform _ownerRoot;

        private float _nextFireTime;
        private bool _isReloading;
        private bool _isAiming;
        private bool _isAimToggleActive;
        private bool _shouldShootFrame;

        private Vector2 _moveInput;
        private Vector2 _lookInput;
        private Vector3 _playerVelocity;
        private bool _isFireHeld;
        private bool _isReloadPressed;
        private bool _isAimPressed;
        private bool _isAimTriggered;

        private CancellationTokenSource _tokenSource;

        private WeaponItemSo _weaponItem;
        private ItemTable _itemTable;

        public WeaponView GetView() => _view;

        public WeaponController(
            WeaponConfig config,
            WeaponModel model,
            WeaponView view,
            IWeaponStatsProvider statsProvider,
            IWeaponViewFactory viewFactory,
            IBulletFactory bulletFactory,
            Transform ownerRoot)
        {
            _config = config;
            _model = model;
            _view = view;
            _statsProvider = statsProvider;
            _viewFactory = viewFactory;
            _bulletFactory = bulletFactory;
            _ownerRoot = ownerRoot;

            _tokenSource = new();
        }

        public WeaponModel Model => _model;

        public void Initialize()
        {
            _model.Initialize(_config.MagazineCapacity, _config.MagazineCapacity, _config.InitialReserveAmmo);
            _view.Initialize(_config);
        }

        public void InitializeFromItem(WeaponItemSo weaponItem, int currentMagazineAmmo, int reserveAmmo)
        {
            _weaponItem = weaponItem;

            _model.Initialize(_config.MagazineCapacity, currentMagazineAmmo, reserveAmmo);
            _view.Initialize(_config);
        }

        public void SetWeaponItem(WeaponItemSo weaponItem) => _weaponItem = weaponItem;

        public WeaponItemSo GetWeaponItem() => _weaponItem;

        public void SetItemTable(ItemTable itemTable) => _itemTable = itemTable;

        public ItemTable GetItemTable() => _itemTable;

        public void SetAmmo(int magazineAmmo, int reserveAmmo) => _model.SetAmmo(magazineAmmo, reserveAmmo);

        public void SetMovementInput(Vector2 input) => _moveInput = input;

        public void SetLookInput(Vector2 input) => _lookInput = input;

        public void SetFireInput(bool isHeld) => _isFireHeld = isHeld;

        public void SetReloadInput(bool isPressed) => _isReloadPressed = isPressed;

        public void SetPlayerVelocity(Vector3 velocity) => _playerVelocity = velocity;

        public void SetAimInput(bool isPressed, bool isTriggered)
        {
            _isAimPressed = isPressed;
            _isAimTriggered = isTriggered;
        }

        public void GetAmmoState(out int currentMagazineAmmo, out int reserveAmmo)
        {
            currentMagazineAmmo = _model.CurrentMagazineAmmo;
            reserveAmmo = _model.ReserveAmmo;
        }

        public void Equip()
        {
            CancelCurrentActions();
            _view.gameObject.SetActive(true);
            _isReloading = false;
            _isAiming = false;
            _isAimToggleActive = false;
            _shouldShootFrame = false;

            _view.ResetVisuals();
            DrawWeaponRoutine().Forget();
        }

        public async UniTask Unequip()
        {
            CancelCurrentActions();

            _view.SetHolsterState(true);

            bool canceled = await UniTask
                .Delay(TimeSpan.FromSeconds(_config.DrawTime), cancellationToken: _tokenSource.Token)
                .SuppressCancellationThrow();

            if (!canceled)
                _view.gameObject.SetActive(false);
        }

        public void Hide()
        {
            CancelCurrentActions();
            _view.ResetVisuals();
            _view.gameObject.SetActive(false);
        }

        public void Dispose()
        {
            CancelCurrentActions();
            _tokenSource.Dispose();
        }

        public void Tick()
        {
            if (!_view.gameObject.activeSelf)
                return;

            HandleAimingState();
            CheckFireInput();
            HandleReload();
            HandleTriggerFinger();
            HandleMovementAnim();
        }

        public void LateTick()
        {
            if (!_view.gameObject.activeSelf)
                return;

            HandleProceduralAnimation();

            if (_shouldShootFrame)
            {
                Fire();
                _shouldShootFrame = false;
            }
        }

        private void HandleProceduralAnimation()
        {
            _view.UpdateProcedural(Time.deltaTime, _lookInput, _isAiming);
        }

        private void HandleAimingState()
        {
            if (_isReloading)
            {
                _isAiming = false;
                _isAimToggleActive = false;
            }
            else
            {
                if (_config.AimIsToggle)
                {
                    if (_isAimTriggered)
                        _isAimToggleActive = !_isAimToggleActive;

                    _isAiming = _isAimToggleActive;
                }
                else
                {
                    _isAiming = _isAimPressed;
                }
            }

            _view.UpdateAiming(_isAiming, _config.AimStability);
        }

        private void CheckFireInput()
        {
            if (_isReloading)
                return;

            if (_isFireHeld && Time.time >= _nextFireTime)
            {
                if (_model.CurrentMagazineAmmo > 0)
                {
                    _shouldShootFrame = true;
                    _nextFireTime = Time.time + _config.FireRate;
                }
            }
        }

        private void Fire()
        {
            if (!_model.TryConsumeAmmo())
                return;

            _view.PlayFireEffects(_isAiming);
            _bulletFactory.SpawnBullet(_view.BulletPrefab, _view.FirePoint.position, _view.FirePoint.rotation,
                _config.Damage, _config.BulletSpeed, _config.InheritVelocity, _playerVelocity, _ownerRoot.gameObject);

            if (_model.IsMagazineEmpty && _model.CanReload)
                ReloadRoutine(true, _config.EmptyReloadDelay).Forget();
        }

        private void HandleReload()
        {
            if (_isReloading)
                return;

            if (_isReloadPressed && _model.CanReload)
                ReloadRoutine(_model.IsMagazineEmpty, 0f).Forget();
        }

        private void HandleMovementAnim()
        {
            bool isMoving = _moveInput.magnitude > 0.1f;
            _view.SetMovementState(isMoving, _moveInput);
        }

        private void HandleTriggerFinger()
        {
            _view.SetTriggerHold(_isFireHeld);
        }

        private async UniTaskVoid ReloadRoutine(bool isEmptyReload, float delayBeforeReload)
        {
            if (_isReloading)
                return;

            if (!_model.CanReload)
                return;

            _isReloading = true;
            _shouldShootFrame = false;

            if (delayBeforeReload > 0f)
            {
                bool delayCanceled = await UniTask
                    .Delay(TimeSpan.FromSeconds(delayBeforeReload), cancellationToken: _tokenSource.Token)
                    .SuppressCancellationThrow();

                if (delayCanceled)
                {
                    _isReloading = false;
                    return;
                }

                if (!_model.CanReload)
                {
                    _isReloading = false;
                    return;
                }
            }

            _view.PlayReload(isEmptyReload);

            bool reloadCanceled = await UniTask
                .Delay(TimeSpan.FromSeconds(_config.ReloadTime), cancellationToken: _tokenSource.Token)
                .SuppressCancellationThrow();

            if (!reloadCanceled)
                _model.Reload();

            _isReloading = false;
        }

        private async UniTaskVoid DrawWeaponRoutine()
        {
            _view.SetHolsterState(false);
            _isReloading = true;

            bool canceled = await UniTask
                .Delay(TimeSpan.FromSeconds(_config.DrawTime), cancellationToken: _tokenSource.Token)
                .SuppressCancellationThrow();

            if (!canceled)
                _isReloading = false;
        }

        private void CancelCurrentActions()
        {
            _tokenSource.Cancel();
            _tokenSource.Dispose();
            _tokenSource = new();
            _isReloading = false;
        }
    }
}
