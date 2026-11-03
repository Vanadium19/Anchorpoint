using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace WeaponModule
{
    /// <summary>
    /// Keeps the list of the single <see cref="WeaponModificationCatalog"/> equal to the set of every
    /// <see cref="WeaponModificationConfig"/> asset in the project, so nobody edits the list by hand. The list is
    /// rebuilt after an import, move or delete of any asset, ordered by id and then by asset name, and the catalog is
    /// saved only when the rebuilt list differs from the stored one. Nothing happens while no catalog exists.
    /// </summary>
    public sealed class WeaponModificationCatalogPostprocessor : AssetPostprocessor
    {
        private const string ConfigFilter = "t:WeaponModificationConfig";
        private const string CatalogFilter = "t:WeaponModificationCatalog";
        private const string ModificationsPropertyName = "modifications";
        private const string AssetExtension = ".asset";

        private static bool _isSynchronizing;

        /// <summary>
        /// Finds the catalog asset, or returns null when the project has none.
        /// </summary>
        public static WeaponModificationCatalog FindCatalog()
        {
            var catalogs = FindAssets<WeaponModificationCatalog>(CatalogFilter);

            return catalogs.Count > 0 ? catalogs[0] : null;
        }

        /// <summary>
        /// Finds every modification config asset of the project, ordered by asset path.
        /// </summary>
        public static List<WeaponModificationConfig> FindConfigs() => FindAssets<WeaponModificationConfig>(ConfigFilter);

        /// <summary>
        /// Returns the configs in catalog order: no nulls, no repeated asset, by id and then by asset name.
        /// </summary>
        public static List<WeaponModificationConfig> BuildOrderedList(IEnumerable<WeaponModificationConfig> configs)
        {
            if (configs == null)
                throw new ArgumentNullException(nameof(configs));

            return configs
                .Where(static config => config != null)
                .Distinct()
                .OrderBy(static config => config.Id ?? string.Empty, StringComparer.Ordinal)
                .ThenBy(static config => config.name, StringComparer.Ordinal)
                .ToList();
        }

        /// <summary>
        /// Returns true when the stored list of the catalog is exactly the ordered list of the given configs.
        /// </summary>
        public static bool IsInSync(WeaponModificationCatalog catalog, IEnumerable<WeaponModificationConfig> configs)
        {
            if (catalog == null)
                throw new ArgumentNullException(nameof(catalog));

            var expected = BuildOrderedList(configs);

            using var serialized = new SerializedObject(catalog);

            return MatchesEntries(RequireProperty(serialized), expected);
        }

        /// <summary>
        /// Rewrites the list of the catalog to the ordered list of the given configs. Returns true when the list
        /// changed, in which case the catalog is marked dirty but not saved. A list that is already correct is not touched.
        /// </summary>
        public static bool Synchronize(WeaponModificationCatalog catalog, IEnumerable<WeaponModificationConfig> configs)
        {
            if (catalog == null)
                throw new ArgumentNullException(nameof(catalog));

            var expected = BuildOrderedList(configs);

            using var serialized = new SerializedObject(catalog);

            var property = RequireProperty(serialized);

            if (MatchesEntries(property, expected))
                return false;

            property.arraySize = expected.Count;

            for (var i = 0; i < expected.Count; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = expected[i];

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);

            return true;
        }

        /// <summary>
        /// Synchronizes the catalog of the project with the config assets and saves it when it changed. Returns
        /// true when the catalog was saved. Does nothing and returns false while the project has no catalog.
        /// </summary>
        public static bool SynchronizeProject()
        {
            var catalog = FindCatalog();

            if (catalog == null || !Synchronize(catalog, FindConfigs()))
                return false;

            AssetDatabase.SaveAssetIfDirty(catalog);

            return true;
        }

        [InitializeOnLoadMethod]
        private static void OnEditorLoaded() => ScheduleSynchronization();

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (ContainsAsset(importedAssets) || ContainsAsset(deletedAssets) || ContainsAsset(movedAssets) ||
                ContainsAsset(movedFromAssetPaths))
            {
                ScheduleSynchronization();
            }
        }

        private static bool ContainsAsset(string[] paths)
        {
            foreach (var path in paths)
            {
                if (path.EndsWith(AssetExtension, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static void ScheduleSynchronization()
        {
            EditorApplication.delayCall -= SynchronizeScheduled;
            EditorApplication.delayCall += SynchronizeScheduled;
        }

        private static void SynchronizeScheduled()
        {
            if (_isSynchronizing || EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                ScheduleSynchronization();

                return;
            }

            _isSynchronizing = true;

            try
            {
                SynchronizeProject();
            }
            finally
            {
                _isSynchronizing = false;
            }
        }

        private static List<T> FindAssets<T>(string filter) where T : UnityEngine.Object
        {
            var paths = AssetDatabase.FindAssets(filter)
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(static path => !string.IsNullOrEmpty(path))
                .OrderBy(static path => path, StringComparer.Ordinal);
            var result = new List<T>();

            foreach (var path in paths)
            {
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);

                if (asset != null)
                    result.Add(asset);
            }

            return result;
        }

        private static SerializedProperty RequireProperty(SerializedObject serialized)
        {
            var property = serialized.FindProperty(ModificationsPropertyName);

            if (property == null || !property.isArray)
            {
                throw new InvalidOperationException(
                    $"'{serialized.targetObject.name}' has no serialized array '{ModificationsPropertyName}'.");
            }

            return property;
        }

        private static bool MatchesEntries(SerializedProperty property, IReadOnlyList<WeaponModificationConfig> expected)
        {
            if (property.arraySize != expected.Count)
                return false;

            for (var i = 0; i < expected.Count; i++)
            {
                if (property.GetArrayElementAtIndex(i).objectReferenceValue != expected[i])
                    return false;
            }

            return true;
        }
    }
}
