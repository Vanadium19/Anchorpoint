using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ECE
{
  public interface IEasyColliderPostProcessor
  {
    /// <summary>
    /// post processes a box collider, properties orientation indiciates if it's a rotated collider.
    /// </summary>
    /// <param name="box"></param>
    /// <param name="properties"></param>
    void PostProcessCollider(BoxCollider boxCollider, EasyColliderProperties properties);

    /// <summary>
    /// post processes a capsule collider, properties orientation indiciates if it's a rotated collider.
    /// </summary>
    /// <param name="box"></param>
    /// <param name="properties"></param>
    void PostProcessCollider(CapsuleCollider capsuleCollider, EasyColliderProperties properties);

    /// <summary>
    /// post processes a mesh collider. cylinder colliders are mesh colliders as well.
    /// </summary>
    /// <param name="box"></param>
    /// <param name="properties"></param>
    void PostProcessCollider(MeshCollider meshCollider, EasyColliderProperties properties);


    /// <summary>
    /// post processes a sphere collider
    /// </summary>
    /// <param name="box"></param>
    /// <param name="properties"></param>
    void PostProcessCollider(SphereCollider sphereCollider, EasyColliderProperties properties);


  }
}