using System.Collections.Generic;

namespace EffectModule
{
    public interface IUseEffectData
    {
        IReadOnlyList<EffectDataSo> Effects { get; }
        IReadOnlyList<BuffDataSo> Buffs { get; }
    }
}
