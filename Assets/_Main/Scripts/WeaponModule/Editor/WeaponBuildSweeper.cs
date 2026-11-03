using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WeaponModule
{
    /// <summary>
    /// Applies builds with one modification (or none) per socket type to a view and checks after each build that
    /// stock parts, attachments, adapters and part meshes are in the state the build asks for. A view whose build space
    /// is larger than the limit is sampled: the empty build, every single modification and an even stride over the rest.
    /// </summary>
    internal static class WeaponBuildSweeper
    {
        public static void Sweep(
            WeaponModificationView view,
            WeaponConfig config,
            IReadOnlyList<WeaponModificationConfig> compatible,
            ISet<WeaponModificationConfig> invalid,
            int maxBuilds,
            WeaponViewReport report)
        {
            var groups = BuildGroups(view, compatible);
            var weights = new long[groups.Count];
            var space = 1L;

            for (var i = 0; i < groups.Count; i++)
            {
                weights[i] = space;
                space = space > long.MaxValue / groups[i].Count ? long.MaxValue : space * groups[i].Count;
            }

            report.BuildSpace = space;

            var originals = RecordOriginalMeshes(view);

            try
            {
                foreach (var index in SelectIndices(groups, weights, space, maxBuilds))
                    ApplyAndCheck(view, config, Decode(groups, weights, index), originals, invalid, report);
            }
            finally
            {
                view.OnBuildChanged(WeaponBuild.Create(config, Array.Empty<WeaponModificationConfig>()));
            }
        }

        private static List<List<WeaponModificationConfig>> BuildGroups(
            WeaponModificationView view,
            IReadOnlyList<WeaponModificationConfig> compatible)
        {
            var groups = new List<List<WeaponModificationConfig>>();
            var seenTypes = new HashSet<WeaponSocketType>();

            foreach (var socket in view.Sockets)
            {
                if (socket == null || socket.Type == null || !seenTypes.Add(socket.Type))
                    continue;

                var group = new List<WeaponModificationConfig> { null };

                group.AddRange(compatible.Where(modification => modification.SocketType == socket.Type));
                groups.Add(group);
            }

            return groups;
        }

        private static List<long> SelectIndices(
            IReadOnlyList<List<WeaponModificationConfig>> groups,
            IReadOnlyList<long> weights,
            long space,
            int maxBuilds)
        {
            var limit = Math.Max(1, maxBuilds);
            var selected = new HashSet<long>();

            if (space <= limit)
            {
                for (var index = 0L; index < space; index++)
                    selected.Add(index);

                return selected.ToList();
            }

            selected.Add(0L);

            for (var group = 0; group < groups.Count && selected.Count < limit; group++)
            {
                for (var digit = 1; digit < groups[group].Count && selected.Count < limit; digit++)
                    selected.Add(digit * weights[group]);
            }

            var step = (double)space / (limit - selected.Count + 1);

            for (var k = 0; selected.Count < limit && k < limit * 2; k++)
                selected.Add((long)(k * step));

            return selected.OrderBy(static index => index).ToList();
        }

        private static List<WeaponModificationConfig> Decode(
            IReadOnlyList<List<WeaponModificationConfig>> groups,
            IReadOnlyList<long> weights,
            long index)
        {
            var installed = new List<WeaponModificationConfig>();

            for (var group = 0; group < groups.Count; group++)
            {
                var digit = (int)(index / weights[group] % groups[group].Count);

                if (groups[group][digit] != null)
                    installed.Add(groups[group][digit]);
            }

            return installed;
        }

        private static Dictionary<Renderer, Mesh> RecordOriginalMeshes(WeaponModificationView view)
        {
            var originals = new Dictionary<Renderer, Mesh>();

            foreach (var socket in view.Sockets)
            {
                if (socket == null)
                    continue;

                foreach (var part in socket.StockParts)
                {
                    if (part == null)
                        continue;

                    foreach (var renderer in WeaponViewValidator.FindPartRenderers(part))
                        originals[renderer] = WeaponViewValidator.GetMesh(renderer);
                }
            }

            return originals;
        }

        private static void ApplyAndCheck(
            WeaponModificationView view,
            WeaponConfig config,
            IReadOnlyList<WeaponModificationConfig> installed,
            IReadOnlyDictionary<Renderer, Mesh> originals,
            ISet<WeaponModificationConfig> invalid,
            WeaponViewReport report)
        {
            var issues = new List<string>();

            try
            {
                view.OnBuildChanged(WeaponBuild.Create(config, installed));
                CheckBuild(view, installed, originals, issues);
            }
            catch (Exception exception)
            {
                issues.Add($"OnBuildChanged threw {exception.GetType().Name}: {exception.Message}");
            }

            report.EvaluatedBuilds++;

            if (issues.Count == 0 && !installed.Any(invalid.Contains))
                report.ValidBuilds++;

            foreach (var issue in issues)
                report.AddIssue(issue);
        }

        private static void CheckBuild(
            WeaponModificationView view,
            IReadOnlyList<WeaponModificationConfig> installed,
            IReadOnlyDictionary<Renderer, Mesh> originals,
            List<string> issues)
        {
            foreach (var socket in view.Sockets)
            {
                if (socket == null || socket.Type == null)
                    continue;

                var modification = installed.FirstOrDefault(candidate => candidate.SocketType == socket.Type);
                var hasAttachment = modification != null && modification.AttachmentPrefab != null && socket.Mount != null;
                var prefix = $"socket '{socket.Type.name}' with {(modification != null ? $"'{modification.Id}'" : "nothing")}";

                CheckStockParts(socket, modification, hasAttachment, originals, prefix, issues);

                if (socket.Mount == null)
                    continue;

                CheckAttachment(socket, modification, hasAttachment, prefix, issues);
                CheckAdapter(socket, hasAttachment, prefix, issues);
            }
        }

        private static void CheckStockParts(
            WeaponModificationSocket socket,
            WeaponModificationConfig modification,
            bool hasAttachment,
            IReadOnlyDictionary<Renderer, Mesh> originals,
            string prefix,
            List<string> issues)
        {
            var hasOperation = modification != null && modification.PartOperation != null && socket.Mount != null;

            foreach (var part in socket.StockParts)
            {
                if (part == null)
                    continue;

                if (part.activeSelf == hasAttachment)
                    issues.Add($"{prefix}: stock part '{part.name}' is {(part.activeSelf ? "active" : "hidden")} but should be {(hasAttachment ? "hidden" : "active")}");

                foreach (var renderer in WeaponViewValidator.FindPartRenderers(part))
                    CheckPartMesh(renderer, originals, hasOperation, prefix, issues);
            }
        }

        private static void CheckPartMesh(
            Renderer renderer,
            IReadOnlyDictionary<Renderer, Mesh> originals,
            bool hasOperation,
            string prefix,
            List<string> issues)
        {
            if (!originals.TryGetValue(renderer, out var original) || original == null)
                return;

            var mesh = WeaponViewValidator.GetMesh(renderer);

            if (hasOperation && (mesh == null || mesh.vertexCount != original.vertexCount))
                issues.Add($"{prefix}: part '{renderer.name}' has no operated mesh of the stock vertex count");
            else if (!hasOperation && mesh != original)
                issues.Add($"{prefix}: part '{renderer.name}' did not get its stock mesh back");
        }

        private static void CheckAttachment(
            WeaponModificationSocket socket,
            WeaponModificationConfig modification,
            bool hasAttachment,
            string prefix,
            List<string> issues)
        {
            var activeCount = 0;
            var isExpectedPrefabActive = false;

            foreach (var attachment in socket.Mount.GetComponentsInChildren<WeaponAttachment>(true))
            {
                if (!attachment.gameObject.activeSelf)
                    continue;

                activeCount++;

                if (hasAttachment && attachment.name.StartsWith(modification.AttachmentPrefab.name, StringComparison.Ordinal))
                    isExpectedPrefabActive = true;
            }

            var expectedCount = hasAttachment ? 1 : 0;

            if (activeCount != expectedCount)
                issues.Add($"{prefix}: {activeCount} attachments are active under the mount, expected {expectedCount}");
            else if (hasAttachment && !isExpectedPrefabActive)
                issues.Add($"{prefix}: the active attachment is not an instance of '{modification.AttachmentPrefab.name}'");
        }

        private static void CheckAdapter(
            WeaponModificationSocket socket,
            bool hasAttachment,
            string prefix,
            List<string> issues)
        {
            if (socket.AdapterPrefab == null)
                return;

            foreach (Transform child in socket.Mount)
            {
                if (!child.name.StartsWith(socket.AdapterPrefab.name, StringComparison.Ordinal))
                    continue;

                if (child.gameObject.activeSelf != hasAttachment)
                    issues.Add($"{prefix}: the adapter is {(child.gameObject.activeSelf ? "active" : "hidden")} but should be {(hasAttachment ? "active" : "hidden")}");

                return;
            }
        }
    }
}
