using UnityEngine;

namespace InventoryModule
{
    [CreateAssetMenu(fileName = "InventoryConfig", menuName = "Configs/Inventory/InventoryConfig")]
    public class InventoryConfig : ScriptableObject
    {
        [Header("Size settings")]
        [SerializeField] private int width = 10;
        [SerializeField] private int height = 5;

        [Header("Discard Settings")]
        [SerializeField] private float discardOffset = 1.5f;
        [SerializeField] private float discardUpOffset = 0.5f;
        [SerializeField] private float discardThrowForce = 2f;

        public int Width => width;
        public int Height => height;
        public float DiscardOffset => discardOffset;
        public float DiscardUpOffset => discardUpOffset;
        public float DiscardThrowForce => discardThrowForce;
    }
}