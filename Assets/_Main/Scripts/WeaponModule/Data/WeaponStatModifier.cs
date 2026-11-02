using System;
using UnityEngine;

namespace WeaponModule
{
    /// <summary>
    /// A single change of a weapon stat. For <see cref="WeaponStatOperation.Multiply"/> the value is a fraction:
    /// 0.25 means +25%, -0.6 means -60%.
    /// </summary>
    [Serializable]
    public struct WeaponStatModifier
    {
        [SerializeField] private WeaponStat stat;
        [SerializeField] private WeaponStatOperation operation;
        [SerializeField] private float value;

        public WeaponStatModifier(WeaponStat stat, WeaponStatOperation operation, float value)
        {
            this.stat = stat;
            this.operation = operation;
            this.value = value;
        }

        public WeaponStat Stat => stat;
        public WeaponStatOperation Operation => operation;
        public float Value => value;
    }
}
