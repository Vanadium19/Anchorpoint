using System;
using System.Collections.Generic;
using UnityEngine;

namespace WeaponModule
{
    /// <summary>
    /// Shows the installed modifications on the weapon. For each socket it finds the installed modification of the
    /// socket type. Its attachment is created once from its prefab under the mount and is only enabled or disabled
    /// afterwards, and it hides the stock parts of the socket. Its part operation swaps the meshes of the stock
    /// parts for operated copies from <see cref="WeaponPartMeshCache"/> and restores the originals on removal.
    /// A modification with neither changes nothing visually.
    /// </summary>
    public class WeaponModificationView : WeaponViewPart
    {
        [SerializeField] private WeaponModificationSocket[] sockets;
        [Tooltip("Tip of the stock front sight. Optional.")]
        [SerializeField] private Transform frontSightPoint;

        private const float MinSightLengthSquared = 1e-8f;

        private readonly Dictionary<(int Socket, WeaponAttachment Prefab), WeaponAttachment> _attachments = new();
        private readonly Dictionary<int, GameObject> _adapters = new();
        private readonly Dictionary<int, StockRenderer[]> _stockRenderers = new();

        private WeaponAttachment _sight;
        private float _sightEyeRelief;

        public Transform FrontSightPoint => frontSightPoint;

        public IReadOnlyList<WeaponModificationSocket> Sockets => sockets ?? Array.Empty<WeaponModificationSocket>();

        public override void OnBuildChanged(WeaponBuild build)
        {
            _sight = null;
            _sightEyeRelief = 0f;

            HideAttachments();

            if (sockets == null)
                return;

            for (var i = 0; i < sockets.Length; i++)
            {
                var socket = sockets[i];

                if (socket == null || socket.Type == null)
                    continue;

                var modification = FindInstalled(build, socket.Type);
                var attachment = ShowAttachment(i, modification);

                ApplyPartOperation(i, modification != null ? modification.PartOperation : null);
                SetStockPartsActive(socket, attachment == null);

                if (attachment != null && attachment.AimPoint != null)
                {
                    _sight = attachment;
                    _sightEyeRelief = socket.EyeRelief;
                }
            }
        }

        /// <summary>
        /// Returns the aim point of the installed sight attachment and the eye relief of its socket, if there is one.
        /// </summary>
        public bool TryGetSight(out Transform aimPoint, out float eyeRelief)
        {
            var hasSight = _sight != null;

            aimPoint = hasSight ? _sight.AimPoint : null;
            eyeRelief = hasSight ? _sightEyeRelief : 0f;

            return hasSight;
        }

        /// <summary>
        /// Moves the dot of the installed sight for the camera. The dot is collimated: it is projected along the sight
        /// axis, which runs from the aim point to the front sight, or along the aim point forward without a front sight.
        /// <paramref name="isAimed"/> keeps the dot visible at the edge of the window. Does nothing without an
        /// installed sight.
        /// </summary>
        public void UpdateReticle(Vector3 cameraWorldPosition, bool isAimed)
        {
            if (_sight == null || _sight.AimPoint == null)
                return;

            var aimPoint = _sight.AimPoint;
            var sightAxis = aimPoint.forward;

            if (frontSightPoint != null)
            {
                var toFrontSight = frontSightPoint.position - aimPoint.position;

                if (toFrontSight.sqrMagnitude > MinSightLengthSquared)
                    sightAxis = toFrontSight;
            }

            _sight.UpdateReticle(cameraWorldPosition, sightAxis, isAimed);
        }

        private static void SetLayerRecursively(Transform root, int layer)
        {
            root.gameObject.layer = layer;

            foreach (Transform child in root)
                SetLayerRecursively(child, layer);
        }

        private static void SetStockPartsActive(WeaponModificationSocket socket, bool isActive)
        {
            foreach (var part in socket.StockParts)
            {
                if (part != null)
                    part.SetActive(isActive);
            }
        }

        private static WeaponModificationConfig FindInstalled(WeaponBuild build, WeaponSocketType socketType)
        {
            var modifications = build.Modifications;

            for (var i = 0; i < modifications.Count; i++)
            {
                if (modifications[i] != null && modifications[i].SocketType == socketType)
                    return modifications[i];
            }

            return null;
        }

        private static StockRenderer[] CollectStockRenderers(WeaponModificationSocket socket)
        {
            var renderers = new List<StockRenderer>();

            foreach (var part in socket.StockParts)
            {
                if (part == null)
                    continue;

                foreach (var skinned in part.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    if (skinned.sharedMesh != null)
                    {
                        renderers.Add(new StockRenderer(
                            null, skinned, skinned.sharedMesh, GetSkinnedMeshToMount(skinned, socket.Mount)));
                    }
                }

                foreach (var filter in part.GetComponentsInChildren<MeshFilter>(true))
                {
                    if (filter.sharedMesh != null && filter.TryGetComponent<MeshRenderer>(out _))
                    {
                        var meshToMount = socket.Mount.worldToLocalMatrix * filter.transform.localToWorldMatrix;

                        renderers.Add(new StockRenderer(filter, null, filter.sharedMesh, meshToMount));
                    }
                }
            }

            return renderers.ToArray();
        }

        private static Matrix4x4 GetSkinnedMeshToMount(SkinnedMeshRenderer renderer, Transform mount)
        {
            var bones = renderer.bones;
            var bindPoses = renderer.sharedMesh.bindposes;
            var boneIndex = FindClosestAncestorBone(bones, mount);

            if (boneIndex < 0 || boneIndex >= bindPoses.Length)
            {
                throw new InvalidOperationException(
                    $"Mount '{mount.name}' is not under any bone of the skinned part '{renderer.name}'.");
            }

            return mount.worldToLocalMatrix * bones[boneIndex].localToWorldMatrix * bindPoses[boneIndex];
        }

        private static int FindClosestAncestorBone(Transform[] bones, Transform mount)
        {
            var closest = -1;
            var closestDepth = -1;

            for (var i = 0; i < bones.Length; i++)
            {
                if (bones[i] == null || !mount.IsChildOf(bones[i]))
                    continue;

                var depth = GetDepth(bones[i]);

                if (depth > closestDepth)
                {
                    closest = i;
                    closestDepth = depth;
                }
            }

            return closest;
        }

        private static int GetDepth(Transform target)
        {
            var depth = 0;

            for (var parent = target.parent; parent != null; parent = parent.parent)
                depth++;

            return depth;
        }

        private void HideAttachments()
        {
            foreach (var attachment in _attachments.Values)
            {
                if (attachment != null)
                    attachment.gameObject.SetActive(false);
            }

            foreach (var adapter in _adapters.Values)
            {
                if (adapter != null)
                    adapter.SetActive(false);
            }
        }

        private WeaponAttachment ShowAttachment(int socketIndex, WeaponModificationConfig modification)
        {
            var socket = sockets[socketIndex];

            if (modification == null || modification.AttachmentPrefab == null || socket.Mount == null)
                return null;

            ShowAdapter(socketIndex);

            var attachment = GetAttachment(socketIndex, modification.AttachmentPrefab);

            attachment.gameObject.SetActive(true);

            return attachment;
        }

        private WeaponAttachment GetAttachment(int socketIndex, WeaponAttachment prefab)
        {
            var key = (socketIndex, prefab);

            if (_attachments.TryGetValue(key, out var attachment) && attachment != null)
                return attachment;

            var mount = sockets[socketIndex].Mount;

            attachment = Instantiate(prefab, mount, false);
            SetLayerRecursively(attachment.transform, mount.gameObject.layer);
            _attachments[key] = attachment;

            return attachment;
        }

        private void ShowAdapter(int socketIndex)
        {
            var prefab = sockets[socketIndex].AdapterPrefab;

            if (prefab == null)
                return;

            if (!_adapters.TryGetValue(socketIndex, out var adapter) || adapter == null)
            {
                var mount = sockets[socketIndex].Mount;

                adapter = Instantiate(prefab, mount, false);
                SetLayerRecursively(adapter.transform, mount.gameObject.layer);
                _adapters[socketIndex] = adapter;
            }

            adapter.SetActive(true);
        }

        private void ApplyPartOperation(int socketIndex, WeaponPartOperation operation)
        {
            var socket = sockets[socketIndex];

            if (!_stockRenderers.TryGetValue(socketIndex, out var renderers))
            {
                if (operation == null || socket.Mount == null)
                    return;

                renderers = CollectStockRenderers(socket);
                _stockRenderers[socketIndex] = renderers;
            }

            for (var i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                var mesh = renderer.Original;

                if (operation != null)
                {
                    var context = new WeaponPartContext(
                        renderer.Original, renderer.MeshToMount, socket.ClearanceAboveAxis, socket.ClearanceBelowAxis);

                    mesh = WeaponPartMeshCache.GetOrCreate(operation, in context);
                }

                renderer.SetMesh(mesh);
            }
        }

        private sealed class StockRenderer
        {
            private readonly MeshFilter _filter;
            private readonly SkinnedMeshRenderer _skinned;

            public StockRenderer(MeshFilter filter, SkinnedMeshRenderer skinned, Mesh original, Matrix4x4 meshToMount)
            {
                _filter = filter;
                _skinned = skinned;
                Original = original;
                MeshToMount = meshToMount;
            }

            public Mesh Original { get; }

            public Matrix4x4 MeshToMount { get; }

            public void SetMesh(Mesh mesh)
            {
                if (_skinned != null)
                {
                    if (_skinned.sharedMesh != mesh)
                        _skinned.sharedMesh = mesh;
                }
                else if (_filter != null && _filter.sharedMesh != mesh)
                {
                    _filter.sharedMesh = mesh;
                }
            }
        }
    }
}
