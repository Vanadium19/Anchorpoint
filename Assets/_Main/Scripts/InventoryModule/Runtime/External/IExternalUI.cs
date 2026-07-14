using UnityEngine;

namespace InventoryModule
{
    public interface IExternalUI
    {
        string DisplayName { get; }
        GameObject UIPrefab { get; }
    }
}
