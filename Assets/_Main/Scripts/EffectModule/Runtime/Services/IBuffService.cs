using System;
using System.Collections.Generic;
using ComponentsModule;

namespace EffectModule
{
    public interface IBuffService
    {
        event Action<ActiveBuff> BuffAdded;
        event Action<ActiveBuff> BuffRemoved;
        event Action<ActiveBuff> BuffUpdated;

        IReadOnlyList<ActiveBuff> GetActiveBuffs(IEntity target);
        void AddBuff(IEntity target, IBuff buff, BuffDataSo buffData = null);
        void RemoveBuff(IEntity target, ActiveBuff activeBuff);
        void RemoveAllBuffs(IEntity target);
        bool HasBuff(IEntity target, string buffId);
    }
}
