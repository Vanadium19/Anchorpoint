using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace WeaponModule
{
    /// <summary>
    /// Turns a <see cref="MeshSplit"/> of a weapon renderer into a stock part: saves both meshes as readable assets, puts
    /// the remainder on the source renderer and creates a sibling renderer for the split-off part with the same
    /// materials and render settings (and the same bones, root bone and bounds for a skinned mesh). The original mesh
    /// asset is never changed. Every scene change is one Undo step; the mesh assets are not removed by Undo.
    /// </summary>
    public static class WeaponPartSplitter
    {
        private const string UndoName = "Create Stock Part";

        /// <summary>
        /// Saves the meshes of <paramref name="split"/> into <paramref name="folder"/> under unique asset names, swaps
        /// the remainder onto <paramref name="source"/> and returns the new sibling renderer that shows the part.
        /// </summary>
        /// <param name="source">A MeshRenderer with a MeshFilter, or a SkinnedMeshRenderer, in a scene or prefab stage.</param>
        /// <param name="split">The split of the mesh of the source renderer.</param>
        /// <param name="partName">Name of the part: the new object and its mesh asset.</param>
        /// <param name="folder">Project folder under Assets that receives the two mesh assets.</param>
        public static Renderer CreateStockPart(Renderer source, MeshSplit split, string partName, string folder)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (split == null)
                throw new ArgumentNullException(nameof(split));

            var sourceMesh = WeaponViewValidator.GetMesh(source);

            if (sourceMesh == null)
                throw new ArgumentException("The source renderer has no mesh.", nameof(source));

            var safeName = SanitizeFileName(partName);

            if (string.IsNullOrEmpty(safeName))
                throw new ArgumentException("The part needs a name.", nameof(partName));

            if (!AssetDatabase.IsValidFolder(folder) || !folder.StartsWith("Assets", StringComparison.Ordinal))
                throw new ArgumentException($"'{folder}' is not a folder under Assets.", nameof(folder));

            try
            {
                var partMesh = SaveMesh(split.Selected, folder, safeName);
                var remainderMesh = SaveMesh(split.Remainder, folder, $"{SanitizeFileName(sourceMesh.name)}_Without_{safeName}");

                return SwapAndCreate(source, partMesh, remainderMesh, partName);
            }
            finally
            {
                DestroyIfUnsaved(split.Selected);
                DestroyIfUnsaved(split.Remainder);
            }
        }

        private static Mesh SaveMesh(Mesh mesh, string folder, string name)
        {
            var path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{name}.asset");

            mesh.name = Path.GetFileNameWithoutExtension(path);
            AssetDatabase.CreateAsset(mesh, path);

            return mesh;
        }

        private static Renderer SwapAndCreate(Renderer source, Mesh partMesh, Mesh remainderMesh, string partName)
        {
            Undo.IncrementCurrentGroup();

            var group = Undo.GetCurrentGroup();
            var parent = ResolveParent(source);
            var partObject = new GameObject(GameObjectUtility.GetUniqueNameForSibling(parent, partName));

            Undo.RegisterCreatedObjectUndo(partObject, UndoName);
            Undo.SetTransformParent(partObject.transform, parent, UndoName);
            CopyPlacement(source, partObject, parent);

            var partRenderer = CloneRenderer(source, partObject, partMesh);

            SetMesh(source, remainderMesh);
            EditorSceneManager.MarkSceneDirty(source.gameObject.scene);
            Undo.CollapseUndoOperations(group);

            return partRenderer;
        }

        private static Transform ResolveParent(Renderer source)
        {
            var parent = source.transform.parent;

            if (parent != null)
                return parent;

            var stage = PrefabStageUtility.GetCurrentPrefabStage();

            return stage != null && stage.prefabContentsRoot == source.gameObject ? source.transform : null;
        }

        private static void CopyPlacement(Renderer source, GameObject target, Transform parent)
        {
            var transform = target.transform;

            if (parent == source.transform.parent)
            {
                transform.localPosition = source.transform.localPosition;
                transform.localRotation = source.transform.localRotation;
                transform.localScale = source.transform.localScale;
                transform.SetSiblingIndex(source.transform.GetSiblingIndex() + 1);
            }
            else
            {
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                transform.localScale = Vector3.one;
            }

            target.layer = source.gameObject.layer;
            target.tag = source.gameObject.tag;
            target.SetActive(source.gameObject.activeSelf);
            GameObjectUtility.SetStaticEditorFlags(target, GameObjectUtility.GetStaticEditorFlags(source.gameObject));
        }

        private static Renderer CloneRenderer(Renderer source, GameObject target, Mesh mesh)
        {
            if (source is SkinnedMeshRenderer skinned)
            {
                var skinnedCopy = Undo.AddComponent<SkinnedMeshRenderer>(target);

                EditorUtility.CopySerialized(skinned, skinnedCopy);
                skinnedCopy.sharedMesh = mesh;

                return skinnedCopy;
            }

            var filter = Undo.AddComponent<MeshFilter>(target);
            var meshRenderer = Undo.AddComponent<MeshRenderer>(target);

            filter.sharedMesh = mesh;
            EditorUtility.CopySerialized(source, meshRenderer);

            return meshRenderer;
        }

        private static void SetMesh(Renderer renderer, Mesh mesh)
        {
            if (renderer is SkinnedMeshRenderer skinned)
            {
                Undo.RecordObject(skinned, UndoName);
                skinned.sharedMesh = mesh;

                return;
            }

            var filter = renderer.GetComponent<MeshFilter>();

            Undo.RecordObject(filter, UndoName);
            filter.sharedMesh = mesh;
        }

        private static string SanitizeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            foreach (var invalid in Path.GetInvalidFileNameChars())
                name = name.Replace(invalid, '_');

            return name.Trim();
        }

        private static void DestroyIfUnsaved(Mesh mesh)
        {
            if (mesh != null && !EditorUtility.IsPersistent(mesh))
                UnityEngine.Object.DestroyImmediate(mesh);
        }
    }
}
