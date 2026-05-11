using ComponentsModule;
using InventoryModule.ContextMenu;
using UnityEngine;

namespace EffectModule
{
    public class BuffTargetInitializer : MonoBehaviour
    {
        [SerializeField] private bool isPrimaryBuffTarget = true;

        private IEntity _entity;

        private void Awake()
        {
            _entity = GetComponent<IEntity>();

            if (_entity == null)
                return;

            if (isPrimaryBuffTarget && _entity.TryGet<IPrimaryBuffTargetService>(out var primaryBuffTargetService))
                primaryBuffTargetService.SetPrimaryTarget(_entity);
        }

        private void Start()
        {
            if (!isPrimaryBuffTarget || _entity == null)
                return;

            if (_entity.TryGet<IContextActionService>(out var contextActionService))
                contextActionService.SetEffectTarget(_entity);
        }
    }
}
