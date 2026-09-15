using System.Collections.Generic;
using UnityEngine;

namespace SpawnModule
{
    /// <summary>Marks a spawnable event site prefab with its guard and loot points and its footprint.</summary>
    /// <remarks>
    /// Authored once on each site prefab (a crash site, an outpost, and so on). An empty
    /// <see cref="GuardPoints"/> or <see cref="LootPoints"/> list tells the spawning action to scatter guards or
    /// loot around the site instead of using fixed points. <see cref="ClearanceRadius"/> and
    /// <see cref="ClearanceHeight"/> describe the space the site itself needs, so a placement search can reject
    /// spots too tight to fit it.
    /// </remarks>
    public class EventSiteView : MonoBehaviour
    {
        [SerializeField] private List<Transform> guardPoints = new();
        [SerializeField] private List<Transform> lootPoints = new();
        [SerializeField] [Min(0f)] private float clearanceRadius = 2f;
        [SerializeField] [Min(0f)] private float clearanceHeight = 3f;

        /// <summary>Points where guarding enemies should be spawned; empty means scatter around the site.</summary>
        public List<Transform> GuardPoints => guardPoints;

        /// <summary>Points where loot should be spawned; empty means scatter around the site.</summary>
        public List<Transform> LootPoints => lootPoints;

        /// <summary>Horizontal radius of the space the site needs kept clear of obstacles.</summary>
        public float ClearanceRadius => clearanceRadius;

        /// <summary>Vertical height of the space the site needs kept clear of obstacles.</summary>
        public float ClearanceHeight => clearanceHeight;
    }
}
