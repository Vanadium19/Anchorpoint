using System;
using UnityEditor;
using UnityEngine;

namespace RandomEventsModule
{
    /// <summary>Renders a <see cref="RandomEventTrigger"/> as the four questions it answers: when, whether, how likely, and out of what.</summary>
    [CustomPropertyDrawer(typeof(RandomEventTrigger))]
    public class RandomEventTriggerDrawer : PropertyDrawer
    {
        /// <inheritdoc/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var sourceProperty = ManagedReferenceEditorGUI.RequireRelative(property, "source");
            var conditionsProperty = ManagedReferenceEditorGUI.RequireRelative(property, "conditions");
            var chanceProperty = ManagedReferenceEditorGUI.RequireRelative(property, "chance");
            var poolProperty = ManagedReferenceEditorGUI.RequireRelative(property, "pool");

            var y = position.y;

            y = DrawReference(position, y, sourceProperty, typeof(IRandomEventTriggerSourceAsset), "When");
            y = DrawProperty(position, y, conditionsProperty, "Only If");
            y = DrawReference(position, y, chanceProperty, typeof(IRandomEventChanceAsset), "Chance");
            DrawProperty(position, y, poolProperty, "Pool");
        }

        /// <inheritdoc/>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var sourceProperty = ManagedReferenceEditorGUI.RequireRelative(property, "source");
            var conditionsProperty = ManagedReferenceEditorGUI.RequireRelative(property, "conditions");
            var chanceProperty = ManagedReferenceEditorGUI.RequireRelative(property, "chance");
            var poolProperty = ManagedReferenceEditorGUI.RequireRelative(property, "pool");

            return ManagedReferenceEditorGUI.GetReferenceFieldHeight(sourceProperty)
                + EditorGUI.GetPropertyHeight(conditionsProperty, true)
                + ManagedReferenceEditorGUI.GetReferenceFieldHeight(chanceProperty)
                + EditorGUI.GetPropertyHeight(poolProperty, true)
                + EditorGUIUtility.standardVerticalSpacing * 3f;
        }

        private static float DrawReference(Rect position, float y, SerializedProperty property, Type baseType, string label)
        {
            var height = ManagedReferenceEditorGUI.GetReferenceFieldHeight(property);
            ManagedReferenceEditorGUI.DrawReferenceField(new Rect(position.x, y, position.width, height), property, baseType, label);

            return y + height + EditorGUIUtility.standardVerticalSpacing;
        }

        private static float DrawProperty(Rect position, float y, SerializedProperty property, string label)
        {
            var height = EditorGUI.GetPropertyHeight(property, true);
            EditorGUI.PropertyField(new Rect(position.x, y, position.width, height), property, new GUIContent(label), true);

            return y + height + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}
