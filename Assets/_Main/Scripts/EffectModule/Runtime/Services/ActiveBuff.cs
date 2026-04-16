namespace EffectModule
{
    public class ActiveBuff
    {
        public IBuff Buff { get; }
        public BuffDataSo BuffData { get; }

        public ActiveBuff(IBuff buff, BuffDataSo buffData)
        {
            Buff = buff;
            BuffData = buffData;
        }
    }
}

