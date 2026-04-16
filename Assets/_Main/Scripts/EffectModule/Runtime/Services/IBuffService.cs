using System;
using System.Collections.Generic;

namespace EffectModule
{
    public interface IBuffService
    {
        event Action<ActiveBuff> BuffAdded;
        event Action<ActiveBuff> BuffRemoved;
        event Action<ActiveBuff> BuffUpdated;

        IReadOnlyList<ActiveBuff> GetActiveBuffs(IBuffTarget target);
        void AddBuff(IBuffTarget target, IBuff buff, BuffDataSo buffData = null);
        void RemoveBuff(IBuffTarget target, ActiveBuff activeBuff);
        void RemoveAllBuffs(IBuffTarget target);
        bool HasBuff(IBuffTarget target, string buffId);
    }
}
