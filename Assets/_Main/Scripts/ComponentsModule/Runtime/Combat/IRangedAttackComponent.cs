using System;
using UnityEngine;

namespace ComponentsModule
{
    public interface IRangedAttackComponent
    {
        event Action Attacked;

        int CurrentAmmo { get; }
        bool IsEmpty { get; }

        bool TryAttack(Vector3 targetPosition, float damage, float projectileSpeed);
        void Reload();
    }
}
