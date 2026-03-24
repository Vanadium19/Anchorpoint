using UnityEngine;

namespace SharedData
{
    public static class EnemyAnimatorHashes
    {
        public static readonly int IsMoving = Animator.StringToHash("IsMoving");
        public static readonly int Shoot = Animator.StringToHash("Shoot");
    }
}
