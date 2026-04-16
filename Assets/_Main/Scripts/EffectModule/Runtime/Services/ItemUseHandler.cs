using InventoryModule;

namespace EffectModule
{
    public class ItemUseHandler : IItemUseHandler
    {
        private readonly IBuffService _buffService;

        public ItemUseHandler(IBuffService buffService)
        {
            _buffService = buffService;
        }

        public void UseItem(ItemTable item, object target)
        {
            if (item == null || target == null)
                return;

            var effectTarget = target as IEffectTarget;

            if (effectTarget == null)
                return;

            var effectData = item.ItemDataSo?.EffectData as IUseEffectData;

            if (effectData == null)
                return;

            foreach (var effect in effectData.Effects)
                effect.Apply(effectTarget);

            if (effectData.Buffs == null)
                return;

            var buffTarget = target as IBuffTarget;

            if (buffTarget == null)
                return;

            foreach (var buff in effectData.Buffs)
                buff.Apply(buffTarget, _buffService);
        }
    }
}
