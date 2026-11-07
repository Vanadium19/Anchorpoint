using UnityEngine;

namespace ECE
{
  public class EasyColliderPostProccessor : IEasyColliderPostProcessor
  {
    // The default implementation does nothing.
    // Can be used to add your own code if you want to do something specifically to each type of collider.
    // If you're creating colliders at runtime with EasyColliderCreator, you can also set the post processor through code,
    // if you don't it will by default use this implementation

    // NOTE: this happens for every single collider created in this asset / created by EasyColliderCreator
    // Vertex selected created colliders, merged colliders, auto skinned colliders, vhacd colliders all go through these methods.

    /// <summary>
    /// post processes a box collider, properties orientation indiciates if it's a rotated collider.
    /// </summary>
    /// <param name="box"></param>
    /// <param name="properties"></param>
    public void PostProcessCollider(BoxCollider boxCollider, EasyColliderProperties properties)
    {
      // in here you'll get passed a completely generated box collider, and can do whatever you want with it.
    }

    /// <summary>
    /// post processes a capsule collider, properties orientation indiciates if it's a rotated collider.
    /// </summary>
    /// <param name="box"></param>
    /// <param name="properties"></param>
    public void PostProcessCollider(CapsuleCollider capsuleCollider, EasyColliderProperties properties)
    {
      // in here you'll get passed a completely generated capsule collider, and can do whatever you want with it.
    }

    /// <summary>
    /// post processes a sphere collider
    /// </summary>
    /// <param name="box"></param>
    /// <param name="properties"></param>
    public void PostProcessCollider(SphereCollider sphereCollider, EasyColliderProperties properties)
    {
      // in here you'll get passed a completely generated sphere collider, and can do whatever you want with it.
    }

    /// <summary>
    /// post processes a mesh collider. cylinder colliders are mesh colliders as well.
    /// </summary>
    /// <param name="box"></param>
    /// <param name="properties"></param>
    public void PostProcessCollider(MeshCollider meshCollider, EasyColliderProperties properties)
    {
      // in here you'll get passed a completely generated mesh collider, and can do whatever you want with it.
    }
  }
}