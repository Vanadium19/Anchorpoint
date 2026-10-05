using UnityEngine;

namespace ComponentsModule
{
    public interface ICombatTarget : IDamageable
    {
        Vector3 Position { get; }
    }
}
