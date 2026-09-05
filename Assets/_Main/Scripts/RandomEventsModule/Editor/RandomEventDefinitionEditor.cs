using UnityEditor;

namespace RandomEventsModule
{
    /// <summary>Custom inspector for <see cref="RandomEventDefinition"/>: lifecycle settings above the action sequence.</summary>
    /// <remarks>The sequence itself is drawn by <see cref="RandomEventActionSequenceDrawer"/>, the same way nested sequences are.</remarks>
    [CustomEditor(typeof(RandomEventDefinition))]
    public class RandomEventDefinitionEditor : Editor
    {
        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawEventSettings();

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(ManagedReferenceEditorGUI.RequireProperty(serializedObject, "sequence"));

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(ManagedReferenceEditorGUI.RequireProperty(serializedObject, "displayNameKey"));

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawEventSettings()
        {
            EditorGUILayout.LabelField("Event Settings", EditorStyles.boldLabel);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(ManagedReferenceEditorGUI.RequireProperty(serializedObject, "isEnabled"));
                EditorGUILayout.PropertyField(ManagedReferenceEditorGUI.RequireProperty(serializedObject, "isExclusive"));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(ManagedReferenceEditorGUI.RequireProperty(serializedObject, "cooldownSeconds"));
            }
        }
    }
}
