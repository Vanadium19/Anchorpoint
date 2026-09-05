using UnityEngine;

namespace RandomEventsModule
{
    /// <summary>
    /// An opaque state key. Conditions, actions and signal relays reference the same key asset
    /// instead of matching hand-typed strings, so a typo becomes impossible.
    /// </summary>
    /// <remarks>The persisted identifier is the asset GUID, so renaming or moving the asset keeps saved state intact.</remarks>
    [CreateAssetMenu(fileName = "RandomEventKey", menuName = "Game/Configs/RandomEvents/RandomEventKey")]
    public class RandomEventKey : ScriptableObject
    {
        [SerializeField] [HideInInspector] private string persistentId;

        /// <summary>The asset's GUID, used as the storage key in <see cref="IRandomEventStateStore"/> and saves.</summary>
        public string PersistentId => persistentId;

#if UNITY_EDITOR
        private void OnValidate()
        {
            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);

            if (string.IsNullOrEmpty(assetPath))
                return;

            var guid = UnityEditor.AssetDatabase.AssetPathToGUID(assetPath);

            if (string.IsNullOrEmpty(guid) || persistentId == guid)
                return;

            persistentId = guid;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
