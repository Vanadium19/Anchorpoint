using UnityEngine;

namespace ComponentsModule
{
    public interface ICoverFinderComponent
    {
        bool TryFindCover(Vector3 threatPosition, float searchRadius, LayerMask obstructionMask, out Vector3 coverPosition);
    }
}