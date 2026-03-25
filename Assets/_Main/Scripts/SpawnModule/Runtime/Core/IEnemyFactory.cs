using UnityEngine;

namespace SpawnModule
{
    public interface IEnemyFactory
    {
        GameObject Create(Vector3 position);
    }
}