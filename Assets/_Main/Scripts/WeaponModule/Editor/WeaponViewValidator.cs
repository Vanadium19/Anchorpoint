using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace WeaponModule
{
    /// <summary>
    /// Checks a weapon view against the modification catalog without any per-weapon or per-modification knowledge.
    /// The same checks run in the build matrix test and behind the Validate button of
    /// <see cref="WeaponModificationViewEditor"/>. Nothing is written: the build sweep runs on a throwaway copy of the
    /// view in a preview scene.
    /// </summary>
    public static class WeaponViewValidator
    {
        /// <summary>
        /// Default limit of builds applied to one view. A view with a larger build space is sampled.
        /// </summary>
        public const int DefaultMaxBuilds = 4096;

        private const float ClearanceTolerance = 1e-4f;

        /// <summary>
        /// Runs every check for one view: its sockets, each compatible modification, and a sweep of builds applied
        /// through <see cref="WeaponViewPart.OnBuildChanged"/> on a copy of the view. A null config is replaced by a
        /// temporary default one.
        /// </summary>
        public static WeaponViewReport Validate(
            WeaponModificationView view,
            IEnumerable<WeaponModificationConfig> modifications,
            WeaponConfig config,
            int maxBuilds = DefaultMaxBuilds)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            if (modifications == null)
                throw new ArgumentNullException(nameof(modifications));

            var all = modifications.Where(static modification => modification != null).ToList();
            var compatible = FindCompatible(view, all);
            var sightTypes = FindSightTypes(all);
            var report = new WeaponViewReport { CompatibleCount = compatible.Count };
            var invalid = new HashSet<WeaponModificationConfig>();

            foreach (var issue in CheckView(view, sightTypes))
                report.AddIssue(issue);

            foreach (var modification in compatible)
            {
                var issues = CheckPair(view, modification, sightTypes);

                if (issues.Count == 0)
                    continue;

                invalid.Add(modification);

                foreach (var issue in issues)
                    report.AddIssue(issue);
            }

            SweepOnCopy(view, config, compatible, invalid, maxBuilds, report);

            return report;
        }

        /// <summary>
        /// Returns the modifications whose socket type the view has a socket for.
        /// </summary>
        public static List<WeaponModificationConfig> FindCompatible(
            WeaponModificationView view,
            IEnumerable<WeaponModificationConfig> modifications)
        {
            var socketTypes = new HashSet<WeaponSocketType>();

            foreach (var socket in view.Sockets)
            {
                if (socket != null && socket.Type != null)
                    socketTypes.Add(socket.Type);
            }

            return modifications
                .Where(modification => modification != null && socketTypes.Contains(modification.SocketType))
                .ToList();
        }

        /// <summary>
        /// Returns the socket types that hold sights: types for which some modification has an attachment with an aim point.
        /// </summary>
        public static HashSet<WeaponSocketType> FindSightTypes(IEnumerable<WeaponModificationConfig> modifications)
        {
            var result = new HashSet<WeaponSocketType>();

            foreach (var modification in modifications)
            {
                if (modification != null && modification.SocketType != null && modification.AttachmentPrefab != null &&
                    modification.AttachmentPrefab.AimPoint != null)
                {
                    result.Add(modification.SocketType);
                }
            }

            return result;
        }

        /// <summary>
        /// Checks the sockets of the view itself: typed, not repeated, no empty stock part, and a front sight point
        /// with an eye relief for every sight socket.
        /// </summary>
        public static List<string> CheckView(WeaponModificationView view, ISet<WeaponSocketType> sightTypes)
        {
            var issues = new List<string>();
            var seenTypes = new HashSet<WeaponSocketType>();
            var sockets = view.Sockets;
            var hasSightSocket = false;

            for (var i = 0; i < sockets.Count; i++)
            {
                var socket = sockets[i];

                if (socket == null || socket.Type == null)
                {
                    issues.Add($"socket {i} has no type");

                    continue;
                }

                if (!seenTypes.Add(socket.Type))
                    issues.Add($"socket type '{socket.Type.name}' is used by more than one socket");

                if (socket.StockParts.Any(static part => part == null))
                    issues.Add($"socket '{socket.Type.name}' has an empty stock part entry");

                if (!sightTypes.Contains(socket.Type))
                    continue;

                hasSightSocket = true;

                if (socket.EyeRelief <= 0f)
                    issues.Add($"sight socket '{socket.Type.name}' has no eye relief");
            }

            if (hasSightSocket && view.FrontSightPoint == null)
                issues.Add("the view has a sight socket but no front sight point");

            return issues;
        }

        /// <summary>
        /// Checks one modification on one view: the mount exists, a sight attachment has an aim point and a reticle, and
        /// the part operation runs on every stock part with the same topology, finite vertices and inside the clearances.
        /// </summary>
        public static List<string> CheckPair(
            WeaponModificationView view,
            WeaponModificationConfig modification,
            ISet<WeaponSocketType> sightTypes)
        {
            var issues = new List<string>();
            var socket = FindSocket(view, modification.SocketType);
            var prefix = $"'{modification.Id}' on socket '{modification.SocketType.name}'";

            if (socket == null)
            {
                issues.Add($"{prefix}: the view has no such socket");

                return issues;
            }

            if (socket.Mount == null)
            {
                issues.Add($"{prefix}: the socket has no mount");

                return issues;
            }

            CheckAttachment(modification, sightTypes, prefix, issues);

            if (modification.PartOperation != null)
                CheckOperation(modification.PartOperation, socket, prefix, issues);

            return issues;
        }

        /// <summary>
        /// Returns the renderers under a stock part that carry a mesh.
        /// </summary>
        public static List<Renderer> FindPartRenderers(GameObject part)
        {
            var result = new List<Renderer>();

            foreach (var renderer in part.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer is SkinnedMeshRenderer || renderer is MeshRenderer && renderer.GetComponent<MeshFilter>() != null)
                    result.Add(renderer);
            }

            return result;
        }

        /// <summary>
        /// Returns the mesh a renderer shows, or null.
        /// </summary>
        public static Mesh GetMesh(Renderer renderer) => renderer switch
        {
            SkinnedMeshRenderer skinned => skinned.sharedMesh,
            MeshRenderer => renderer.GetComponent<MeshFilter>().sharedMesh,
            _ => null
        };

        private static WeaponModificationSocket FindSocket(WeaponModificationView view, WeaponSocketType type)
        {
            foreach (var socket in view.Sockets)
            {
                if (socket != null && socket.Type == type)
                    return socket;
            }

            return null;
        }

        private static void CheckAttachment(
            WeaponModificationConfig modification,
            ISet<WeaponSocketType> sightTypes,
            string prefix,
            List<string> issues)
        {
            var attachment = modification.AttachmentPrefab;

            if (attachment == null)
                return;

            var isSight = sightTypes.Contains(modification.SocketType);

            if (isSight && attachment.AimPoint == null)
                issues.Add($"{prefix}: the attachment sits on a sight socket but has no aim point");

            if (attachment.AimPoint == null)
                return;

            if (attachment.Reticle == null)
                issues.Add($"{prefix}: the attachment has an aim point but no reticle");
            else if (!attachment.Reticle.IsChildOf(attachment.AimPoint))
                issues.Add($"{prefix}: the reticle is not a child of the aim point");
        }

        private static void CheckOperation(
            WeaponPartOperation operation,
            WeaponModificationSocket socket,
            string prefix,
            List<string> issues)
        {
            if (socket.StockParts.Count == 0)
            {
                issues.Add($"{prefix}: the part operation needs stock parts but the socket has none");

                return;
            }

            foreach (var part in socket.StockParts)
            {
                if (part == null)
                    continue;

                var renderers = FindPartRenderers(part);

                if (renderers.Count == 0)
                    issues.Add($"{prefix}: stock part '{part.name}' has no mesh renderer");

                foreach (var renderer in renderers)
                    CheckOperationOnRenderer(operation, socket, renderer, $"{prefix}, part '{renderer.name}'", issues);
            }
        }

        private static void CheckOperationOnRenderer(
            WeaponPartOperation operation,
            WeaponModificationSocket socket,
            Renderer renderer,
            string prefix,
            List<string> issues)
        {
            var source = GetMesh(renderer);

            if (source == null)
                return;

            if (!source.isReadable)
            {
                issues.Add($"{prefix}: mesh '{source.name}' is not readable");

                return;
            }

            if (!TryCreateMeshToMount(renderer, socket.Mount, source, out var meshToMount, out var error))
            {
                issues.Add($"{prefix}: {error}");

                return;
            }

            var context = new WeaponPartContext(source, meshToMount, socket.ClearanceAboveAxis, socket.ClearanceBelowAxis);
            Mesh result;

            try
            {
                result = operation.Apply(in context);
            }
            catch (Exception exception)
            {
                issues.Add($"{prefix}: Apply threw {exception.GetType().Name}: {exception.Message}");

                return;
            }

            try
            {
                CheckResult(source, result, meshToMount, socket, prefix, issues);
            }
            finally
            {
                if (result != null && result != source && !EditorUtility.IsPersistent(result))
                    Object.DestroyImmediate(result);
            }
        }

        private static bool TryCreateMeshToMount(
            Renderer renderer,
            Transform mount,
            Mesh source,
            out Matrix4x4 meshToMount,
            out string error)
        {
            meshToMount = Matrix4x4.identity;
            error = null;

            if (renderer is not SkinnedMeshRenderer skinned)
            {
                meshToMount = mount.worldToLocalMatrix * renderer.transform.localToWorldMatrix;

                return true;
            }

            var bones = skinned.bones;
            var bindposes = source.bindposes;
            var boneIndex = FindMountBone(bones, mount);

            if (boneIndex < 0 || boneIndex >= bindposes.Length)
            {
                error = "the mount is not a descendant of any bone of the skinned mesh";

                return false;
            }

            meshToMount = mount.worldToLocalMatrix * bones[boneIndex].localToWorldMatrix * bindposes[boneIndex];

            return true;
        }

        private static int FindMountBone(IReadOnlyList<Transform> bones, Transform mount)
        {
            var bestIndex = -1;
            var bestDepth = -1;

            for (var i = 0; i < bones.Count; i++)
            {
                if (bones[i] == null || !mount.IsChildOf(bones[i]))
                    continue;

                var depth = GetDepth(bones[i]);

                if (depth > bestDepth)
                {
                    bestIndex = i;
                    bestDepth = depth;
                }
            }

            return bestIndex;
        }

        private static int GetDepth(Transform transform)
        {
            var depth = 0;

            for (var parent = transform.parent; parent != null; parent = parent.parent)
                depth++;

            return depth;
        }

        private static void CheckResult(
            Mesh source,
            Mesh result,
            Matrix4x4 meshToMount,
            WeaponModificationSocket socket,
            string prefix,
            List<string> issues)
        {
            if (result == null)
            {
                issues.Add($"{prefix}: Apply returned null");

                return;
            }

            if (!CheckTopology(source, result, prefix, issues))
                return;

            var sourceVertices = source.vertices;
            var resultVertices = result.vertices;
            var resultNormals = result.normals;
            var nonFiniteCount = 0;
            var aboveCount = 0;
            var belowCount = 0;
            var maxAboveExcess = 0f;
            var maxBelowExcess = 0f;

            for (var i = 0; i < resultVertices.Length; i++)
            {
                var vertex = resultVertices[i];

                if (!IsFinite(vertex) || i < resultNormals.Length && !IsFinite(resultNormals[i]))
                {
                    nonFiniteCount++;

                    continue;
                }

                var inMount = meshToMount.MultiplyPoint3x4(vertex);
                var stockInMount = meshToMount.MultiplyPoint3x4(sourceVertices[i]);
                var aboveExcess = socket.ClearanceAboveAxis > 0f
                    ? inMount.y - Mathf.Max(socket.ClearanceAboveAxis, stockInMount.y)
                    : 0f;
                var belowExcess = socket.ClearanceBelowAxis > 0f
                    ? Mathf.Min(-socket.ClearanceBelowAxis, stockInMount.y) - inMount.y
                    : 0f;

                if (aboveExcess > ClearanceTolerance)
                    aboveCount++;

                if (belowExcess > ClearanceTolerance)
                    belowCount++;

                maxAboveExcess = Mathf.Max(maxAboveExcess, aboveExcess);
                maxBelowExcess = Mathf.Max(maxBelowExcess, belowExcess);
            }

            if (nonFiniteCount > 0)
                issues.Add($"{prefix}: {nonFiniteCount} vertices or normals are NaN or infinite");

            if (aboveCount > 0)
            {
                issues.Add($"{prefix}: {aboveCount} vertices leave the clearance above the axis " +
                           $"({socket.ClearanceAboveAxis:0.####} m), by up to {maxAboveExcess:0.####} m");
            }

            if (belowCount > 0)
            {
                issues.Add($"{prefix}: {belowCount} vertices leave the clearance below the axis " +
                           $"({socket.ClearanceBelowAxis:0.####} m), by up to {maxBelowExcess:0.####} m");
            }
        }

        private static bool CheckTopology(Mesh source, Mesh result, string prefix, List<string> issues)
        {
            var isSame = true;

            if (result.vertexCount != source.vertexCount)
            {
                issues.Add($"{prefix}: vertex count changed from {source.vertexCount} to {result.vertexCount}");
                isSame = false;
            }

            if (result.subMeshCount != source.subMeshCount)
            {
                issues.Add($"{prefix}: submesh count changed from {source.subMeshCount} to {result.subMeshCount}");
                isSame = false;
            }
            else
            {
                for (var i = 0; i < source.subMeshCount; i++)
                {
                    if (result.GetIndexCount(i) == source.GetIndexCount(i))
                        continue;

                    issues.Add($"{prefix}: index count of submesh {i} changed");
                    isSame = false;
                }
            }

            if (result.bindposeCount != source.bindposeCount)
            {
                issues.Add($"{prefix}: bind pose count changed from {source.bindposeCount} to {result.bindposeCount}");
                isSame = false;
            }

            return isSame;
        }

        private static bool IsFinite(Vector3 value) =>
            float.IsFinite(value.x) && float.IsFinite(value.y) && float.IsFinite(value.z);

        private static void SweepOnCopy(
            WeaponModificationView view,
            WeaponConfig config,
            IReadOnlyList<WeaponModificationConfig> compatible,
            ISet<WeaponModificationConfig> invalid,
            int maxBuilds,
            WeaponViewReport report)
        {
            var temporaryConfig = config == null ? ScriptableObject.CreateInstance<WeaponConfig>() : null;
            var shouldClearCache = WeaponPartMeshCache.Count == 0 && !Application.isPlaying;

            try
            {
                using var copy = ViewCopy.Create(view);

                WeaponBuildSweeper.Sweep(copy.View, config != null ? config : temporaryConfig, compatible, invalid,
                    maxBuilds, report);
            }
            catch (Exception exception)
            {
                report.AddIssue($"the build sweep failed: {exception.GetType().Name}: {exception.Message}");
            }
            finally
            {
                if (temporaryConfig != null)
                    Object.DestroyImmediate(temporaryConfig);

                if (shouldClearCache)
                    WeaponPartMeshCache.Clear();
            }
        }

        private sealed class ViewCopy : IDisposable
        {
            private readonly GameObject _root;
            private readonly Scene _previewScene;
            private readonly bool _isLoadedPrefab;

            private ViewCopy(GameObject root, Scene previewScene, bool isLoadedPrefab, WeaponModificationView view)
            {
                _root = root;
                _previewScene = previewScene;
                _isLoadedPrefab = isLoadedPrefab;
                View = view;
            }

            public WeaponModificationView View { get; }

            public static ViewCopy Create(WeaponModificationView source)
            {
                if (EditorUtility.IsPersistent(source))
                {
                    var sourceRoot = source.transform.root;
                    var loaded = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(sourceRoot.gameObject));

                    return new ViewCopy(loaded, default, true, FindCounterpart(sourceRoot, source.transform, loaded.transform));
                }

                var weaponView = source.GetComponentInParent<WeaponView>(true);
                var copiedRoot = weaponView != null ? weaponView.transform : source.transform.root;
                var scene = EditorSceneManager.NewPreviewScene();
                var clone = Object.Instantiate(copiedRoot.gameObject);

                SceneManager.MoveGameObjectToScene(clone, scene);

                return new ViewCopy(clone, scene, false, FindCounterpart(copiedRoot, source.transform, clone.transform));
            }

            public void Dispose()
            {
                if (_isLoadedPrefab)
                {
                    PrefabUtility.UnloadPrefabContents(_root);

                    return;
                }

                if (_root != null)
                    Object.DestroyImmediate(_root);

                EditorSceneManager.ClosePreviewScene(_previewScene);
            }

            private static WeaponModificationView FindCounterpart(Transform sourceRoot, Transform source, Transform copyRoot)
            {
                var path = new Stack<int>();

                for (var current = source; current != sourceRoot; current = current.parent)
                    path.Push(current.GetSiblingIndex());

                var target = copyRoot;

                foreach (var index in path)
                    target = target.GetChild(index);

                return target.GetComponent<WeaponModificationView>();
            }
        }
    }
}
