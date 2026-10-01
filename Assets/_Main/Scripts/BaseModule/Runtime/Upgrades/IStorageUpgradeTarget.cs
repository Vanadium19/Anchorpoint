using UnityEngine;

namespace BaseModule
{
    public interface IStorageUpgradeTarget
    {
        /// <summary>
        /// Current size of the main storage grid.
        /// </summary>
        Vector2Int StorageSize { get; }

        /// <summary>
        /// Grows the main storage grid by the given number of columns and rows.
        /// </summary>
        void ExpandStorage(Vector2Int increase);
    }
}
