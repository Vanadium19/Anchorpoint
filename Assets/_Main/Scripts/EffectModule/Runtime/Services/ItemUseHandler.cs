using ComponentsModule;
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

            var entity = target as IEntity;

            if (entity == null)
                return;

            var effectData = item.ItemDataSo?.EffectData as IUseEffectData;

            if (effectData == null)
                return;

            foreach (var effect in effectData.Effects)
                effect.Apply(entity);

            if (effectData.Buffs == null)
                return;

            foreach (var buff in effectData.Buffs)
                buff.Apply(entity, _buffService);
        }
    }
}
