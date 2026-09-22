using System;
using System.Collections.Generic;
using UnityEngine;

namespace BaseModule
{
    [Serializable]
    public class ChestUpgradeLevel
    {
        [SerializeField] private List<Vector2Int> gridSizes = new();

        public IReadOnlyList<Vector2Int> GridSizes => gridSizes;
    }
}
