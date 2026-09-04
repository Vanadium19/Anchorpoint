using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace RandomEventsModule
{
    /// <summary>
    /// Rect-based drawing shared by every editor in the module that renders a
    /// <c>[SerializeReference]</c> field as "type-picker button + the picked type's own fields":
    /// action steps in <see cref="RandomEventDefinitionEditor"/> and condition entries in
    /// <see cref="RandomEventConditionSetDrawer"/>. Keeps both in sync instead of duplicating the
    /// same rect math and dropdown wiring twice.
    /// </summary>
    public static class ManagedReferenceEditorGUI
    {
        /// <summary>Height of one type-picker button row.</summary>
        public static float HeaderHeight => EditorGUIUtility.singleLineHeight;

        /// <summary>Label for the reference's type-picker button.</summary>
        public static string GetLabel(SerializedProperty referenceProperty) =>
            referenceProperty.managedReferenceValue is IRandomEventAsset asset ? asset.EditorLabel : "<None>";

        /// <summary>Total height needed to draw the picked type's own fields.</summary>
        public static float GetBodyHeight(SerializedProperty referenceProperty)
        {
            if (referenceProperty.managedReferenceValue == null)
                return 0f;

            var height = 0f;
            var iterator = referenceProperty.Copy();
            var end = iterator.GetEndProperty();
            var enterChildren = true;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
            {
                height += EditorGUI.GetPropertyHeight(iterator, true) + EditorGUIUtility.standardVerticalSpacing;
                enterChildren = false;
            }

            return height;
        }

        /// <summary>Draws the picked type's own fields inside the given rect.</summary>
        public static void DrawBody(Rect rect, SerializedProperty referenceProperty)
        {
            if (referenceProperty.managedReferenceValue == null)
                return;

            var iterator = referenceProperty.Copy();
            var end = iterator.GetEndProperty();
            var enterChildren = true;
            var y = rect.y;

            while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, end))
            {
                var height = EditorGUI.GetPropertyHeight(iterator, true);
                EditorGUI.PropertyField(new Rect(rect.x, y, rect.width, height), iterator, true);
                y += height + EditorGUIUtility.standardVerticalSpacing;
                enterChildren = false;
            }
        }

        /// <summary>Opens the searchable type picker for a <c>[SerializeReference]</c> field.</summary>
        public static void ShowTypeDropdown(Rect rect, Type baseType, Action<Type> onSelected) =>
            new ManagedReferenceTypeDropdown(new AdvancedDropdownState(), baseType, onSelected).Show(rect);

        /// <summary>Replaces a <c>[SerializeReference]</c> field's value with a new instance of the given type.</summary>
        public static void SetReference(SerializedProperty referenceProperty, Type type)
        {
            referenceProperty.serializedObject.Update();
            referenceProperty.managedReferenceValue = type == null ? null : Activator.CreateInstance(type);
            referenceProperty.serializedObject.ApplyModifiedProperties();
        }

        /// <summary>Finds a serialized property by name, or throws if a rename left the editor out of sync.</summary>
        public static SerializedProperty RequireProperty(SerializedObject serializedObjectRef, string name) =>
            serializedObjectRef.FindProperty(name)
                ?? throw new InvalidOperationException($"'{serializedObjectRef.targetObject.GetType().Name}' has no serialized field '{name}' — a rename left an editor out of sync.");

        /// <summary>Finds a relative serialized property by name, or throws if a rename left the editor out of sync.</summary>
        public static SerializedProperty RequireRelative(SerializedProperty property, string name) =>
            property.FindPropertyRelative(name)
                ?? throw new InvalidOperationException($"'{property.propertyPath}' has no relative field '{name}' — a rename left an editor out of sync.");
    }
}
