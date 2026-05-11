using System.Collections.Generic;
using ComponentsModule;
using UnityEngine;
using Zenject;
using EffectModule;

namespace UIModule
{
    public class BuffsView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform slotsContainer;
        [SerializeField] private BuffSlotView slotPrefab;

        private readonly Dictionary<string, BuffSlotView> _slots = new();
        private IBuffService _buffService;
        private IEntity _target;

        [Inject]
        private void Construct(IBuffService buffService)
        {
            _buffService = buffService;

            if (_buffService != null)
            {
                _buffService.BuffAdded += OnBuffAdded;
                _buffService.BuffRemoved += OnBuffRemoved;
                _buffService.BuffUpdated += OnBuffUpdated;
            }
        }

        public void Initialize(IEntity target)
        {
            _target = target;
        }

        private void OnDestroy()
        {
            if (_buffService != null)
            {
                _buffService.BuffAdded -= OnBuffAdded;
                _buffService.BuffRemoved -= OnBuffRemoved;
                _buffService.BuffUpdated -= OnBuffUpdated;
            }
        }

        private void OnBuffAdded(ActiveBuff activeBuff)
        {
            var buffId = activeBuff.Buff.BuffId + "_" + activeBuff.Buff.GetHashCode();
            
            if (_slots.ContainsKey(buffId))
                return;

            var slot = Instantiate(slotPrefab, slotsContainer);
            _slots[buffId] = slot;
            
            if (activeBuff.BuffData?.Icon != null)
                slot.SetIcon(activeBuff.BuffData.Icon);
            
            UpdateSlot(slot, activeBuff);
        }

        private void OnBuffRemoved(ActiveBuff activeBuff)
        {
            var buffId = activeBuff.Buff.BuffId + "_" + activeBuff.Buff.GetHashCode();

            if (!_slots.TryGetValue(buffId, out var slot))
                return;

            _slots.Remove(buffId);
            Destroy(slot.gameObject);
        }

        private void OnBuffUpdated(ActiveBuff activeBuff)
        {
            var buffId = activeBuff.Buff.BuffId + "_" + activeBuff.Buff.GetHashCode();

            if (!_slots.TryGetValue(buffId, out var slot))
                return;

            UpdateSlot(slot, activeBuff);
        }

        private void UpdateSlot(BuffSlotView slot, ActiveBuff activeBuff)
        {
            slot.SetDuration(activeBuff.Buff.RemainingTime, activeBuff.Buff.Duration);
        }
    }
}
