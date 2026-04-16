using System;
using Newtonsoft.Json;
using SaveModule;

namespace EffectModule
{
    public class BuffSaveable : ISaveable
    {
        private const string Key = "player_buffs";

        private readonly IBuffService _buffService;
        private readonly IPrimaryBuffTargetService _primaryBuffTargetService;
        private readonly BuffCatalog _buffCatalog;

        public string SaveKey => Key;

        public BuffSaveable(
            IBuffService buffService,
            IPrimaryBuffTargetService primaryBuffTargetService,
            BuffCatalog buffCatalog)
        {
            _buffService = buffService;
            _primaryBuffTargetService = primaryBuffTargetService;
            _buffCatalog = buffCatalog;
        }

        public string CreateMemento()
        {
            var buffTarget = _primaryBuffTargetService?.PrimaryTarget;

            if (buffTarget == null)
                return "{}";

            var activeBuffs = _buffService.GetActiveBuffs(buffTarget);
            var memento = new BuffsMemento();

            foreach (var activeBuff in activeBuffs)
            {
                var buffMemento = new BuffMemento
                {
                    BuffId = activeBuff.Buff.BuffId,
                    RemainingTime = activeBuff.Buff.RemainingTime,
                    Duration = activeBuff.Buff.Duration,
                    BuffDataName = activeBuff.BuffData != null ? activeBuff.BuffData.name : null
                };

                memento.Buffs.Add(buffMemento);
            }

            return JsonConvert.SerializeObject(memento);
        }

        public void RestoreMemento(string data)
        {
            var buffTarget = _primaryBuffTargetService?.PrimaryTarget;

            if (buffTarget == null)
                return;

            RestoreInternal(buffTarget, data);
        }

        private void RestoreInternal(IBuffTarget buffTarget, string data)
        {
            var memento = JsonConvert.DeserializeObject<BuffsMemento>(data);

            if (memento == null || memento.Buffs == null)
                return;

            foreach (var buffMemento in memento.Buffs)
                RestoreBuff(buffTarget, buffMemento);
        }

        private void RestoreBuff(IBuffTarget buffTarget, BuffMemento memento)
        {
            var buffData = _buffCatalog?.GetByName(memento.BuffDataName);

            if (buffData == null || buffData.BuffEffects == null || buffData.BuffEffects.Count == 0)
                return;

            var buff = buffData.BuffEffects[0].CreateBuff(memento.Duration);
            var elapsed = memento.Duration - memento.RemainingTime;
            _buffService.AddBuff(buffTarget, buff, buffData);
            buff.SetElapsedTime(elapsed);
        }
    }
}
