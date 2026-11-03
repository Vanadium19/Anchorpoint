using System;
using UnityEditor;
using UnityEngine;

namespace WeaponModule
{
    /// <summary>
    /// Inspector of <see cref="WeaponModificationView"/>: the default fields, a Create Mount button for every socket
    /// without a mount, and a Validate button that runs <see cref="WeaponViewValidator"/>, the same checks as the build
    /// matrix test, against the modification catalog.
    /// </summary>
    [CustomEditor(typeof(WeaponModificationView))]
    public sealed class WeaponModificationViewEditor : Editor
    {
        private const string SocketsPropertyName = "sockets";
        private const string TypePropertyName = "type";
        private const string MountPropertyName = "mount";
        private const string UndoName = "Create Socket Mount";
        private const int MaxShownIssues = 30;

        private Transform _mountParent;
        private WeaponViewReport _report;
        private string _failure;

        private void OnEnable()
        {
            _report = null;
            _failure = null;
        }

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            DrawMountTools();
            EditorGUILayout.Space();
            DrawValidation();
        }

        private void DrawMountTools()
        {
            EditorGUILayout.LabelField("Socket mounts", EditorStyles.boldLabel);
            _mountParent = (Transform)EditorGUILayout.ObjectField(
                new GUIContent("Mount parent", "Parent of a new mount. The view itself when empty."),
                _mountParent,
                typeof(Transform),
                true);

            serializedObject.Update();

            var sockets = serializedObject.FindProperty(SocketsPropertyName);

            for (var i = 0; i < sockets.arraySize; i++)
            {
                var socket = sockets.GetArrayElementAtIndex(i);
                var type = socket.FindPropertyRelative(TypePropertyName).objectReferenceValue;
                var mount = socket.FindPropertyRelative(MountPropertyName);
                var label = type != null ? type.name : $"Socket {i}";

                using (new EditorGUI.DisabledScope(type == null || mount.objectReferenceValue != null))
                {
                    if (GUILayout.Button($"Create Mount: {label}"))
                        CreateMount(mount, label);
                }
            }
        }

        private void CreateMount(SerializedProperty mountProperty, string socketName)
        {
            var view = (WeaponModificationView)target;
            var parent = _mountParent != null ? _mountParent : view.transform;
            var mount = new GameObject(GameObjectUtility.GetUniqueNameForSibling(parent, $"{socketName}Mount"));

            Undo.RegisterCreatedObjectUndo(mount, UndoName);
            Undo.SetTransformParent(mount.transform, parent, UndoName);
            mount.layer = parent.gameObject.layer;
            PlaceMount(mount.transform, parent, view);

            mountProperty.objectReferenceValue = mount.transform;
            serializedObject.ApplyModifiedProperties();
        }

        private static void PlaceMount(Transform mount, Transform parent, WeaponModificationView view)
        {
            var weaponView = view.GetComponentInParent<WeaponView>(true);
            var firePoint = weaponView != null ? weaponView.FirePoint : null;
            var rotation = firePoint != null ? firePoint.rotation : view.transform.rotation;
            var local = parent.worldToLocalMatrix * Matrix4x4.TRS(parent.position, rotation, Vector3.one);

            mount.localPosition = local.GetPosition();
            mount.localRotation = local.rotation;
            mount.localScale = local.lossyScale;
        }

        private void DrawValidation()
        {
            EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);

            if (GUILayout.Button("Validate"))
                Validate();

            if (!string.IsNullOrEmpty(_failure))
                EditorGUILayout.HelpBox(_failure, MessageType.Error);

            if (_report == null)
                return;

            var buildText = _report.IsSampled
                ? $"{_report.EvaluatedBuilds} of {_report.BuildSpace} builds sampled"
                : $"{_report.BuildSpace} builds";

            EditorGUILayout.HelpBox(
                $"{_report.CompatibleCount} compatible modifications, {buildText}, {_report.ValidShare:P1} valid.",
                _report.Issues.Count == 0 ? MessageType.Info : MessageType.Warning);

            for (var i = 0; i < _report.Issues.Count && i < MaxShownIssues; i++)
                EditorGUILayout.HelpBox(_report.Issues[i], MessageType.Error);

            if (_report.Issues.Count > MaxShownIssues)
                EditorGUILayout.HelpBox($"{_report.Issues.Count - MaxShownIssues} more problems.", MessageType.Error);
        }

        private void Validate()
        {
            _report = null;
            _failure = null;

            var catalog = WeaponModificationCatalogPostprocessor.FindCatalog();

            if (catalog == null)
            {
                _failure = "The project has no WeaponModificationCatalog asset.";

                return;
            }

            try
            {
                _report = WeaponViewValidator.Validate((WeaponModificationView)target, catalog.Modifications, null);
            }
            catch (Exception exception)
            {
                _failure = $"{exception.GetType().Name}: {exception.Message}";
            }
        }
    }
}
