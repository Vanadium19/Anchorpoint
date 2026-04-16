using System;
using UnityEngine;

namespace InventoryModule
{
    [Serializable]
    public class DurabilityMetadata : InventoryMetadata
    {
        public event Action<int, int> DurabilityChanged;

        public int Current { get; private set; }
        public int Max { get; private set; }

        public bool IsDepleted => Current <= 0;
        public float Normalized => Max > 0 ? (float)Current / Max : 0f;

        public override void Initialize(ItemTable itemTable)
        {
            base.Initialize(itemTable);

            if (itemTable?.ItemDataSo == null)
                return;

            Max = itemTable.ItemDataSo.MaxDurability;
            Current = Max;
        }

        public void SetInitialValues(int max)
        {
            Max = max;
            Current = max;
        }

        public void RestoreValues(int current, int max)
        {
            Max = max;
            Current = current;
        }

        public int Consume(int amount)
        {
            if (IsDepleted || amount <= 0)
                return 0;

            int consumed = Mathf.Min(amount, Current);
            Current = Mathf.Max(0, Current - consumed);

            DurabilityChanged?.Invoke(Current, Max);

            return consumed;
        }

        public void Add(int amount)
        {
            Current = Mathf.Min(Max, Current + amount);
            DurabilityChanged?.Invoke(Current, Max);
        }

        public void SetCurrent(int value)
        {
            Current = Mathf.Clamp(value, 0, Max);
            DurabilityChanged?.Invoke(Current, Max);
        }
    }
}
