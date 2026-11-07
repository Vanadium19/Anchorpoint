using UnityEngine;
namespace ECE
{
  /// <summary>
  /// Properties to use when creating a collider.
  /// </summary>
  public struct EasyColliderProperties
  {
    /// <summary>
    /// Marks the collider's isTrigger property
    /// </summary>
    public bool IsTrigger;

    /// <summary>
    /// Layer of gameobject when creating a rotated collider.
    /// </summary>
    public int Layer;

#if (UNITY_6000_0_OR_NEWER)
    /// <summary>
    /// Physic material to set on collider.
    /// </summary>
    public PhysicsMaterial PhysicMaterial;
#else
    /// <summary>
    /// Physic material to set on collider.
    /// </summary>
    public PhysicMaterial PhysicMaterial;
    public void SetPhysicMat(PhysicMaterial physmat)
    {
      PhysicMaterial = physmat;
    }
#endif


#if (UNITY_2022_2_OR_NEWER)
    /// <summary>
    /// whether or not the collidere generates contacts for physics.contact event.
    /// </summary>
    public bool ProvidesContacts;
    /// <summary>
    /// layer overrides - layer override priority property on a collider.
    /// </summary>
    public int LayerOverridePriority;

    /// <summary>
    /// layer overrides exclude layer mask property on colliders.
    /// </summary>

    public LayerMask ExcludeLayers;

    /// <summary>
    /// layer overrides include layer mask property on colliders.
    /// </summary>
    public LayerMask IncludeLayers;
#endif

    /// <summary>
    /// Orientation of created collider.
    /// </summary>
    public COLLIDER_ORIENTATION Orientation;

    /// <summary>
    /// Gameobject collider gets added to.
    /// </summary>
    public GameObject AttachTo;


    /// <summary>
    /// Properties with which to create a collider
    /// </summary>
    /// <param name="isTrigger">Should the collider's isTrigger property be true?</param>
    /// <param name="layer">Layer of gameobject when creating a rotated collider</param>
    /// <param name="physicMaterial">Physic Material to apply to a collider</param>
    /// <param name="attachTo">GameObject to attach the collider to</param>
    /// <param name="orientation">Orientation of the collider for generation</param>
    public EasyColliderProperties(bool isTrigger, int layer,
#if (UNITY_6000_0_OR_NEWER)
    PhysicsMaterial physicMaterial,
#else
    PhysicMaterial physicMaterial,
#endif

    GameObject attachTo, COLLIDER_ORIENTATION orientation = COLLIDER_ORIENTATION.NORMAL)
    {
      IsTrigger = isTrigger;
      Layer = layer;
      PhysicMaterial = physicMaterial;
      AttachTo = attachTo;
      Orientation = orientation;
#if (UNITY_2022_2_OR_NEWER)
      ProvidesContacts = false;
      LayerOverridePriority = 0;
      IncludeLayers = 0;
      ExcludeLayers = 0;
#endif
    }

  }
}
