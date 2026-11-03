using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Localization;

namespace WeaponModule.Tests
{
    /// <summary>
    /// Validates every weapon item and every modification config of the project against each other, so a new weapon or
    /// modification needs no test of its own. Reads assets through the AssetDatabase and writes nothing; views are
    /// checked on throwaway copies that <see cref="WeaponViewValidator"/> destroys.
    /// </summary>
    public class WeaponBuildMatrixTests
    {
        private const string LocalizationTableName = "Localization";
        private const string WeaponFilter = "t:WeaponItemSo";

        private WeaponModificationCatalog _catalog;
        private List<WeaponModificationConfig> _configs;
        private List<WeaponItemSo> _weapons;

        [OneTimeSetUp]
        public void LoadAssets()
        {
            _catalog = WeaponModificationCatalogPostprocessor.FindCatalog();
            _configs = WeaponModificationCatalogPostprocessor.FindConfigs();
            _weapons = AssetDatabase.FindAssets(WeaponFilter)
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(static path => path, System.StringComparer.Ordinal)
                .Select(AssetDatabase.LoadAssetAtPath<WeaponItemSo>)
                .Where(static weapon => weapon != null)
                .ToList();
        }

        [Test]
        public void Catalog_ListsEveryConfigAssetOnce()
        {
            Assert.IsNotNull(_catalog, "The project has no WeaponModificationCatalog asset.");

            var listed = _catalog.Modifications.ToList();
            var issues = new List<string>();

            foreach (var config in _configs.Where(config => !listed.Contains(config)))
                issues.Add($"'{config.name}' is not in the catalog");

            foreach (var entry in listed.Where(entry => entry == null || !_configs.Contains(entry)))
                issues.Add(entry == null ? "the catalog has an empty entry" : $"'{entry.name}' is in the catalog but is not a project asset");

            foreach (var duplicate in listed.Where(static entry => entry != null).GroupBy(static entry => entry).Where(static group => group.Count() > 1))
                issues.Add($"'{duplicate.Key.name}' is in the catalog {duplicate.Count()} times");

            if (issues.Count == 0 && !WeaponModificationCatalogPostprocessor.IsInSync(_catalog, _configs))
                issues.Add("the catalog entries are not in the order the postprocessor writes");

            AssertNoIssues(issues, "The catalog is out of sync with the config assets");
        }

        [Test]
        public void Modifications_HaveUniqueIdsKeysCostsAndIcons()
        {
            var collection = LocalizationEditorSettings.GetStringTableCollection(LocalizationTableName);
            var issues = new List<string>();

            if (collection == null)
                issues.Add($"the project has no string table collection '{LocalizationTableName}'");

            foreach (var group in _configs.GroupBy(static config => config.Id ?? string.Empty).Where(static group => group.Count() > 1 && group.Key.Length > 0))
                issues.Add($"id '{group.Key}' is used by {string.Join(", ", group.Select(static config => config.name))}");

            foreach (var config in _configs)
                CheckConfig(config, collection, issues);

            AssertNoIssues(issues, "Modification configs have problems");
        }

        [Test]
        public void SocketTypes_OfModificationsExistOnSomeWeapon()
        {
            var orphans = _configs
                .Where(config => config.SocketType != null && !_weapons.Any(weapon => weapon.HasSocket(config.SocketType)))
                .Select(static config => $"'{config.Id}' needs socket type '{config.SocketType.name}', which no weapon has");

            foreach (var orphan in orphans)
                TestContext.WriteLine($"WARNING {orphan}");
        }

        [Test]
        public void EveryWeaponAndCompatibleModification_PassesValidation()
        {
            Assume.That(_weapons, Is.Not.Empty, "The project has no weapon items.");
            Assert.IsNotNull(_catalog, "The project has no WeaponModificationCatalog asset.");

            var issues = new List<string>();
            var compatiblePairs = 0;
            var buildSpace = 0L;
            var evaluated = 0;
            var valid = 0;
            var sampled = false;

            foreach (var weapon in _weapons)
            {
                var report = ValidateWeapon(weapon, _catalog.Modifications, issues);

                if (report == null)
                    continue;

                compatiblePairs += report.CompatibleCount;
                buildSpace += report.BuildSpace;
                evaluated += report.EvaluatedBuilds;
                valid += report.ValidBuilds;
                sampled |= report.IsSampled;

                TestContext.WriteLine(
                    $"{weapon.name}: {report.CompatibleCount} compatible modifications, build space {report.BuildSpace}, " +
                    $"{report.ValidBuilds} of {report.EvaluatedBuilds} evaluated builds valid");

                issues.AddRange(report.Issues.Select(issue => $"[{weapon.name}] {issue}"));
            }

            TestContext.WriteLine(
                $"Weapons: {_weapons.Count}. Modifications: {_catalog.Modifications.Count}. Compatible pairs: {compatiblePairs}.");
            TestContext.WriteLine(
                $"Build space: {buildSpace} builds over all weapons{(sampled ? " (evaluated by sampling)" : string.Empty)}. " +
                $"Valid builds: {valid} of {evaluated} evaluated ({(evaluated > 0 ? 100d * valid / evaluated : 0d):0.##}%).");

            AssertNoIssues(issues, "Weapon and modification combinations have problems");
        }

        private static WeaponViewReport ValidateWeapon(
            WeaponItemSo weapon,
            IEnumerable<WeaponModificationConfig> modifications,
            List<string> issues)
        {
            var view = weapon.WeaponViewPrefab != null ? weapon.WeaponViewPrefab.ModificationView : null;

            if (view == null)
            {
                issues.Add($"[{weapon.name}] the weapon has no view prefab with a modification view");

                return null;
            }

            return WeaponViewValidator.Validate(view, modifications, weapon.WeaponConfig);
        }

        private static void CheckConfig(WeaponModificationConfig config, StringTableCollection collection, List<string> issues)
        {
            var name = $"'{config.name}'";

            if (string.IsNullOrWhiteSpace(config.Id))
                issues.Add($"{name} has an empty id");

            if (config.SocketType == null)
                issues.Add($"{name} has no socket type");

            if (config.Icon == null)
                issues.Add($"{name} has no icon");

            CheckKey(name, "name key", config.NameKey, collection, issues);
            CheckKey(name, "description key", config.DescriptionKey, collection, issues);

            if (config.Costs.Count == 0)
                TestContext.WriteLine($"WARNING {name} has no costs");

            foreach (var cost in config.Costs)
            {
                if (cost == null || cost.Item == null || cost.Count <= 0)
                    issues.Add($"{name} has a cost without an item or with a count below 1");
            }
        }

        private static void CheckKey(string owner, string label, string key, StringTableCollection collection, List<string> issues)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                issues.Add($"{owner} has an empty {label}");

                return;
            }

            if (collection == null)
                return;

            if (!collection.SharedData.Contains(key))
            {
                issues.Add($"{owner} {label} '{key}' is not in the string table '{collection.TableCollectionName}'");

                return;
            }

            foreach (var table in collection.StringTables)
            {
                var entry = table.GetEntry(key);

                if (entry == null || string.IsNullOrEmpty(entry.Value))
                    TestContext.WriteLine($"WARNING {owner} {label} '{key}' has no text for locale {table.LocaleIdentifier.Code}");
            }
        }

        private static void AssertNoIssues(IReadOnlyCollection<string> issues, string headline)
        {
            if (issues.Count > 0)
                Assert.Fail($"{headline} ({issues.Count}):\n{string.Join("\n", issues)}");
        }
    }
}
