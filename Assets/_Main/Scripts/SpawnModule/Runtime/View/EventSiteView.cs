using System.Collections.Generic;
using UnityEngine;

namespace SpawnModule
{
    /// <summary>Marks a spawnable event site prefab with its unit and loot points and its footprint.</summary>
    /// <remarks>
    /// Authored once on each site prefab (a crash site, an outpost, a trader camp, and so on). An empty
    /// <see cref="UnitPoints"/> or <see cref="LootPoints"/> list tells the spawning action to scatter that
    /// group around the site instead of using fixed points.
    /// <see cref="ClearanceRadius"/> and <see cref="ClearanceHeight"/> describe the space the site itself
    /// needs, so a placement search can reject spots too tight to fit it.
    /// </remarks>
    public class EventSiteView : MonoBehaviour
    {
        [SerializeField] private List<Transform> unitPoints = new();
        [SerializeField] private List<Transform> lootPoints = new();
        [SerializeField] [Min(0f)] private float clearanceRadius = 2f;
        [SerializeField] [Min(0f)] private float clearanceHeight = 3f;

        /// <summary>Points where the site's units (guards, occupants, or both) should be spawned; empty means scatter around the site.</summary>
        public List<Transform> UnitPoints => unitPoints;

        /// <summary>Points where loot should be spawned; empty means scatter around the site.</summary>
        public List<Transform> LootPoints => lootPoints;

        /// <summary>Horizontal radius of the space the site needs kept clear of obstacles.</summary>
        public float ClearanceRadius => clearanceRadius;

        /// <summary>Vertical height of the space the site needs kept clear of obstacles.</summary>
        public float ClearanceHeight => clearanceHeight;
    }
}
