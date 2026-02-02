using UnityEngine;

namespace ComponentsModule
{
    public interface IPlayerPositionProvider
    {
        Vector3 Position { get; }
        Quaternion Rotation { get; }
        Vector3 Forward { get; }
    }
}