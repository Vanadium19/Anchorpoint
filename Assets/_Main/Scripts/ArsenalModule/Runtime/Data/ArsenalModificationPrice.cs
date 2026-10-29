using System;
using System.Collections.Generic;
using UnityEngine;
using WeaponModule;

namespace ArsenalModule
{
    /// <summary>
    /// The price of installing a single weapon modification at the Arsenal.
    /// </summary>
    [Serializable]
    public class ArsenalModificationPrice
    {
        [SerializeField] private WeaponModificationConfig modification;
        [SerializeField] private List<ArsenalCost> cost = new();

        /// <summary>
        /// The modification this price applies to.
        /// </summary>
        public WeaponModificationConfig Modification => modification;

        /// <summary>
        /// Items and amounts the player pays to install <see cref="Modification"/>.
        /// </summary>
        public IReadOnlyList<ArsenalCost> Cost => cost;
    }
}
