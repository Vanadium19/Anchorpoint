using UnityEngine;
using ComponentsModule;
using InventoryModule;
using InventoryModule.ContextMenu;
using WeaponModule;
using Zenject;

namespace EffectModule
{
    public class BuffTarget : MonoInstaller, IBuffTarget, IEffectTarget
    {
        [SerializeField] private bool isPrimaryBuffTarget = true;

        private IHealthComponent _healthComponent;
        private IMoveComponent _moveComponent;
        private IWeaponInventory _weaponInventory;
        private IPrimaryBuffTargetService _primaryBuffTargetService;

        public Transform Transform => transform;

        public bool TryGet<T>(out T component) where T : class
        {
            if (typeof(T) == typeof(IHealthComponent))
            {
                component = _healthComponent as T;
                return _healthComponent != null;
            }

            if (typeof(T) == typeof(IMoveComponent))
            {
                component = _moveComponent as T;
                return _moveComponent != null;
            }

            if (typeof(T) == typeof(IWeaponInventory))
            {
                component = _weaponInventory as T;
                return _weaponInventory != null;
            }

            if (this is T self)
            {
                component = self;
                return true;
            }

            var found = GetComponent<T>();
            component = found;

            return found != null;
        }

        [Inject]
        private void Construct(IHealthComponent healthComponent, IMoveComponent moveComponent, IWeaponInventory weaponInventory)
        {
            _healthComponent = healthComponent;
            _moveComponent = moveComponent;
            _weaponInventory = weaponInventory;
        }

        [Inject]
        private void Construct(IPrimaryBuffTargetService primaryBuffTargetService)
        {
            _primaryBuffTargetService = primaryBuffTargetService;

            if (isPrimaryBuffTarget)
                _primaryBuffTargetService.SetPrimaryTarget(this);
        }

        public override void InstallBindings()
        {
            Container.Bind<IBuffTarget>()
                .FromInstance(this)
                .AsSingle();

            Container.Bind<IEffectTarget>()
                .FromInstance(this)
                .AsSingle();
        }

        public override void Start()
        {
            if (!isPrimaryBuffTarget)
                return;
                
            var contextActionService = Container.TryResolve<IContextActionService>();
            contextActionService?.SetEffectTarget(this);
        }
    }
}
