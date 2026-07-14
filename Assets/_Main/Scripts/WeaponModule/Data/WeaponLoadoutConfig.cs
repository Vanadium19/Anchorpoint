using System.Collections.Generic;
using UnityEngine;

namespace WeaponModule
{
    [CreateAssetMenu(fileName = "WeaponLoadout", menuName = "Configs/WeaponLoadout")]
    public class WeaponLoadoutConfig : ScriptableObject
    {
        [SerializeField] private List<WeaponItemSo> weapons;

        public IReadOnlyList<WeaponItemSo> Weapons => weapons;
    }
}
