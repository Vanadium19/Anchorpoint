using UnityEditor;
using UnityEngine;

namespace RandomEventsModule
{
    /// <summary>Renders a <see cref="RandomEventPoolEntry"/> as one row: the event asset and the weight this pool gives it.</summary>
    [CustomPropertyDrawer(typeof(RandomEventPoolEntry))]
    public class RandomEventPoolEntryDrawer : PropertyDrawer
    {
        private const float WeightWidth = 54f;
        private const float WeightLabelWidth = 46f;

        /// <inheritdoc/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var definitionProperty = ManagedReferenceEditorGUI.RequireRelative(property, "definition");
            var weightProperty = ManagedReferenceEditorGUI.RequireRelative(property, "weight");

            var rowRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            var definitionRect = new Rect(rowRect.x, rowRect.y, rowRect.width - WeightWidth - WeightLabelWidth - RandomEventEditorStyles.ListElementSpacing, rowRect.height);
            var weightLabelRect = new Rect(definitionRect.xMax + RandomEventEditorStyles.ListElementSpacing, rowRect.y, WeightLabelWidth, rowRect.height);
            var weightRect = new Rect(weightLabelRect.xMax, rowRect.y, WeightWidth, rowRect.height);

            EditorGUI.PropertyField(definitionRect, definitionProperty, GUIContent.none);
            EditorGUI.LabelField(weightLabelRect, "Weight");
            EditorGUI.PropertyField(weightRect, weightProperty, GUIContent.none);
        }

        /// <inheritdoc/>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;
    }
}
