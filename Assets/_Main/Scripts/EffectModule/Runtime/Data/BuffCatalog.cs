using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EffectModule
{
    [CreateAssetMenu(fileName = "BuffCatalog", menuName = "Game/Configs/Effect/BuffCatalog")]
    public class BuffCatalog : ScriptableObject
    {
        [SerializeField] private List<BuffDataSo> buffs;

        private Dictionary<string, BuffDataSo> _buffCache;

        public BuffDataSo GetByName(string name)
        {
            BuildCache();
            return _buffCache.TryGetValue(name, out var buff) ? buff : null;
        }

        private void BuildCache()
        {
            if (_buffCache != null)
                return;

            _buffCache = new Dictionary<string, BuffDataSo>();

            foreach (var buff in buffs)
            {
                if (buff != null && !_buffCache.ContainsKey(buff.name))
                    _buffCache[buff.name] = buff;
            }
        }
    }
}
